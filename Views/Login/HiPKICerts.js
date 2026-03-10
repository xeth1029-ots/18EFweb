/* 搭配 HiPKILocalSignServer 運作的自然人憑證登入驗證功能 */

// console logging (for IE8 compatible)
var console = console || { "log": function () { }, "debug": function () { }, "error": function () { } };

var HiPKULocalServer = "http://localhost:61161";
var MSG_NotInstallPKILocalServer = "您的電腦尚未安裝【中華電信自然人憑證跨平台網頁元件】!";
var TBS = "13141806";

var _responseTimeout = 7000;
var _UA = window.navigator.userAgent;
var _isIE = false;
var _postTarget;
var _timeoutId;
var _reqFunc;
var _reqPIN;

/* _getUserCert() 若成功讀取到使用者憑證, 會將 記錄在這個變數中 */
var _CertData = {
        label: "",      /*憑證名稱: cert1, cert2, ... */
        usage: "",      /*憑證使用類型: digitalSignature, keyEncipherment|dataEncipherment */
        subjectDN: "",  /*自然人憑證所有人 DN*/
        subjectID: "",  /*身份證末4碼*/
        certB64: "",    /*簽章用(usage=digitalSignature)憑證資料(Base64格式)*/
        signedData: ""  /*以簽章憑證 Signed 的資料*/
    };

var _getCertCallback;
var _makeSignCallback;

/*清除 _CertData */
function _resetCertData() {
    _CertData.label = "";
    _CertData.usage = "";
    _CertData.subjectDN = "";
    _CertData.subjectID = "";
    _CertData.certB64 = "";
    _CertData.signedData = "";
}

/* create ActiveX http component */
function _createHttpObject() {
    var elemDiv = document.createElement('div');
    elemDiv.innerHTML = '<OBJECT id="http" width=1 height=1 style="LEFT: 1px; TOP: 1px" type="application/x-httpcomponent" VIEWASTEXT></OBJECT>';
    document.body.appendChild(elemDiv);
}

/*(for IE) send http POST via ActiveX http component */
function _setSignatureIE(target, data) {
    var http = document.getElementById("http");
    if (!http || !http.sendRequest) {
        return null;
    }
    http.url = target;
    http.actionMethod = "POST";
    var code = http.sendRequest(data);
    if (code != 0) return null;
    return http.responseText;
}
function _checkFinish() {
    if (_postTarget) {
        _postTarget.close();
        alert(MSG_NotInstallPKILocalServer);
    }
}
/* 
憑證登入 PIN 碼檢核, 結果以 callback(certData, rtnCode, msg) 回應,
若成功: certData=憑證資訊, rtnCode=0
若失敗: rtnCode!=0, Msg=錯誤訊息
*/
function CertValidation(userPIN, callback) {
    _resetCertData();

    if (!callback || !(callback instanceof Function)) {
        console.error("CertValidation(): callback is undefined");
        return;
    }

    if (!userPIN) {
        callback(null, -2, "沒有輸入 PIN 碼!");
        return;
    }

    var timer = setTimeout(function () {
            callback(null, -9999, "回應逾時!\n" + MSG_NotInstallPKILocalServer);
        }, 10000);

    /* 1. 使用憑證進行簽章 API 呼叫 (用來檢驗 PIN 碼是否正確) */
    _makeSignature(userPIN, function (signedData, rtnCode, msg) {
        clearTimeout(timer);

        if (rtnCode == 0) {
            // PIN 碼驗證成功
            _CertData.signedData = signedData;

            /* 2. 讀取憑證資料 API 呼叫 */
            _getUserCert(function (rtnCode, msg) {
                if (rtnCode == 0) {
                    // 讀取憑證資料成功
                    callback(_CertData, 0, null);
                }
                else {
                    callback(null, rtnCode, msg);
                }
            });
        }
        else {
            // 簽章失敗(PIN 碼驗證失敗)
            callback(null, rtnCode, msg);
        }
    });
}

function _makeSignature(userPIN, callback) {
    _reqFunc = "MakeSignature";
    _reqPIN = userPIN;
    _SignedData = "";
    _makeSignCallback = callback;

    if (_isIE) //is IE, use ActiveX
    {
        _postTarget = window.open(HiPKULocalServer + "/waiting.gif", "Signing", "height=200, width=200, left=100, top=20");
        var data = _setSignatureIE(HiPKULocalServer + "/sign", "tbsPackage=" + _getTbsPackage() );
        _postTarget.close();
        _postTarget = null;
        if (!data) alert(MSG_NotInstallPKILocalServer);
        else _setSignature(data);
    }
    else {
        _postTarget = window.open(HiPKULocalServer + "/popupForm", "簽章中", "height=200, width=200, left=100, top=20");
        _timeoutId = setTimeout(_checkFinish, _responseTimeout);
    }
}
function _getUserCert( callback ) {
    _reqFunc = "GetUserCert";
    _resetCertData();
    _getCertCallback = callback;

    if (_isIE) //is IE, use ActiveX
    {
        _postTarget = window.open(HiPKULocalServer + "/waiting.gif", "Reading", "height=200, width=200, left=100, top=20");
        var data = _setSignature(HiPKULocalServer + "/pkcs11info?withcert=true", "");
        _postTarget.close();
        _postTarget = null;
        if (!data) alert(MSG_NotInstallPKILocalServer);
        else _setUserCert(data);
    }
    else {
        _postTarget = window.open(HiPKULocalServer + "/popupForm", "憑證讀取中", "height=200, width=200, left=100, top=20");
        _timeoutId = setTimeout(_checkFinish, _responseTimeout);
    }
}
function _triggerMakeSignCallback(signedData, rtnCode, msg) {
    if (_makeSignCallback) {
        _makeSignCallback(signedData, rtnCode, msg);
    }
    else {
        console.log("_makeSignCallback undefied!");
    }
}
/*解析收到的簽章資料, 設定: _CertReturnCode, _SignedData */
function _setSignature(signature) {
    var ret = JSON.parse(signature);
    var _SignedData = ret.signature;

    if (ret.ret_code != 0) {
        var errMsg = MajorErrorReason(ret.ret_code);
        if (ret.last_error)
            errMsg += " " + MinorErrorReason(ret.last_error);

        _triggerMakeSignCallback(null, ret.ret_code, errMsg);
    }
    else {
        _triggerMakeSignCallback(_SignedData, 0, null);
    }
}
function _triggerGetCertCallback(certData, rtnCode, msg) {
    if (_getCertCallback) {
        _getCertCallback(certData, rtnCode, msg);
    }
    else {
        console.log("_getCertCallback undefied!");
    }
}
/*解析收到的憑證資料, 設定: _CertReturnCode, _CertData */
function _setUserCert(certData) {
    var ret = JSON.parse(certData);

    if (ret.ret_code != 0) {
        var errMsg = MajorErrorReason(ret.ret_code);
        if (ret.last_error)
            errMsg += " " + MinorErrorReason(ret.last_error);

        _triggerGetCertCallback(ret.ret_code, errMsg);
        return;
    }
    var usage = "digitalSignature";     //"keyEncipherment|dataEncipherment";
    var slots = ret.slots;
    for (var index in slots) {
        if (slots[index].token == null || slots[index].token === "unknown token") continue;
        var certs = slots[index].token.certs;
        for (var indexCert in certs) {
            console.log(certs[indexCert].label
                + ": " + certs[indexCert].usage
                + ": " + certs[indexCert].subjectDN
                + ": " + certs[indexCert].subjectID);
            if (certs[indexCert].usage == usage) {
                _CertData.label = certs[indexCert].label;
                _CertData.usage = certs[indexCert].usage;
                _CertData.subjectDN = certs[indexCert].subjectDN;
                _CertData.subjectID = certs[indexCert].subjectID;
                _CertData.certB64 = certs[indexCert].certb64;

                _triggerGetCertCallback(0, null);
                return;
            }
        }
    }
    _triggerGetCertCallback(-1, "找不到憑證, 請確認憑證有正確插入讀卡機!");
}
/* 根據 _reqFunc 產生呼叫 HiPKILocalServer 的 WebSocket Request Package */
function _getTbsPackage() {
    var tbsData = {};

    if (_reqFunc == "GetUserCert") {
        tbsData = { "func": "GetUserCert" };
    }
    else if (_reqFunc == "MakeSignature") {
        tbsData["func"] = "MakeSignature";
        tbsData["signatureType"] = "PKCS7";
        tbsData["tbs"] = "TBS";
        tbsData["tbsEncoding"] = "";
        tbsData["hashAlgorithm"] = "SHA256";
        tbsData["withCardSN"] = "";
        tbsData["pin"] = _reqPIN;
        tbsData["nonce"] = "";
    }
    return JSON.stringify(tbsData);
}

/* _receiveMessage 接收並處理來自 HiPKULocalServer 的回應訊息 */
function _receiveMessage(event) {
    if (console) console.debug(event);

    //安全起見，這邊應填入網站位址檢查
    if (event.origin != HiPKULocalServer)
        return;
    try {
        var ret = JSON.parse(event.data);
        if (ret.func) {
            if (ret.func == "getTbs") {
                clearTimeout(_timeoutId);

                var json = _getTbsPackage();
                _postTarget.postMessage(json, "*");
            }
            else if (ret.func == "sign") {
                // MakeSignature 的 message event
                _setSignature(event.data);
            }
            else if (ret.func == "pkcs11info") {
                // GetUserCert 的 message event
                _setUserCert(event.data);
            }
        } else {
            if (console) console.error("no func");
        }
    } catch (e) {
        //errorhandle
        if (console) console.error(e);
    }
}
/* WebSocket 標準: 
   綁定 window.message event 接收來自 HiPKULocalServer 的回應訊息 
*/
if (window.addEventListener) {
    window.addEventListener("message", _receiveMessage, false);
} else {
    //for IE8
    window.attachEvent("onmessage", _receiveMessage);
}

if (_UA.indexOf("MSIE") != -1 || _UA.indexOf("Trident") != -1) 
{
    //is IE, use ActiveX http component
    _isIE = true;
    _createHttpObject();
}
