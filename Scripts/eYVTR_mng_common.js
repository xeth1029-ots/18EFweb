
/* 判斷欄位值 */
function chkValue(strFlag, strName, obj1, obj2, obj3) {
    var strMsg = '';

    switch (strFlag) {
        case 'empty': //判斷必填欄位
            if (isBlank(obj1)) strMsg = '請輸入' + strName + '!\n';
            else if (obj2 != null && isBlank(obj2)) strMsg = '請輸入' + strName + '!\n';
            else if (obj3 != null && isBlank(obj3)) strMsg = '請輸入' + strName + '!\n';
            break;
        case 'select': //判斷必選下拉選單
            if (isBlank(obj1)) strMsg = '請選擇' + strName + '!\n';
            break;
        case 'int': //判斷非必填數字欄位
            if (!isBlank(obj1) && !isUnsignedInt(obj1.value)) strMsg = strName + '格式有誤，請輸入整數!\n';
            break;
        case 'int_must': //判斷必填數字欄位
            if (isBlank(obj1)) strMsg = '請輸入' + strName + '!\n';
            else if (!isUnsignedInt(obj1.value)) strMsg = strName + '格式有誤，請輸入整數!\n';
            break;
        case 'date': //判斷非必填日期欄位
            if (!isBlank(obj1) && !checkRocDate(obj1.value)) strMsg = strName + '格式有誤，請輸入正確日期格式!\n';
            break;
        case 'date_must': //判斷必填日期欄位
            if (isBlank(obj1)) strMsg = '請輸入' + strName + '!\n';
            else if (!checkRocDate(obj1.value)) strMsg = strName + '格式有誤，請輸入正確日期格式!\n';
            break;
        case 'yearrange': //判斷非必填年度
            if (!isBlank(obj1) && !isBlank(obj2)) {
                if (obj1.value > obj2.value) strMsg += strName + '起年' + '不可大於' + strName + '迄年!\n';
            }
            break;
        case 'yearrange_must': //判斷必填年度
            if (isBlank(obj1)) strMsg += '請選擇' + strName + '起年!\n';
            if (isBlank(obj2)) strMsg += '請選擇' + strName + '迄年!\n';

            if (!isBlank(obj1) && !isBlank(obj2)) {
                if (Number(obj1.value) > Number(obj2.value)) strMsg += strName + '起年' + '不可大於' + strName + '迄年!\n';
            }
            break;

        case 'yearmonth_must': //判斷必填年度月份
            if (isBlank(obj1)) strMsg += '請選擇' + strName + '年度!\n';
            if (isBlank(obj2)) strMsg += '請選擇' + strName + '起月!\n';
            if (isBlank(obj3)) strMsg += '請選擇' + strName + '迄月!\n';

            if (!isBlank(obj2) && !isBlank(obj3)) {
                if (Number(obj2.value) > Number(obj3.value)) strMsg += strName + '起月' + '不可大於' + strName + '迄月!\n';
            }
            break;
        case 'daterange': //判斷非必填日期區間
            if (!isBlank(obj1) && !checkRocDate(obj1.value)) strMsg += strName + '起日格式有誤，請輸入正確日期格式!\n';
            if (!isBlank(obj2) && !checkRocDate(obj2.value)) strMsg += strName + '迄日格式有誤，請輸入正確日期格式!\n';

            if (checkRocDate(obj1.value) && checkRocDate(obj2.value)) {
                if (getDiffDay(getAdDate(obj1.value), getAdDate(obj2.value)) < 0) strMsg += strName + '迄日不可小於起日!\n';
            }
            break;
        case 'daterange_must': //判斷必填日期區間
            if (isBlank(obj1)) strMsg = '請輸入' + strName + '起日!\n';
            else if (!checkRocDate(obj1.value)) strMsg += strName + '起日格式有誤，請輸入正確日期格式!\n';

            if (isBlank(obj2)) strMsg = '請輸入' + strName + '迄日!\n';
            else if (!checkRocDate(obj2.value)) strMsg += strName + '迄日格式有誤，請輸入正確日期格式!\n';

            if (checkRocDate(obj1.value) && checkRocDate(obj2.value)) {
                if (getDiffDay(getAdDate(obj1.value), getAdDate(obj2.value)) < 0) strMsg += strName + '迄日不可小於起日!\n';
            }
            break;
            //非常用新增區
            //↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
        case 'chgpass': //判斷修改密碼
            if (isBlank(obj1)) strMsg += '請輸入舊密碼!\n';
            //else if (obj1.value.length < 12 || obj1.value.length > 16) strMsg += '舊密碼請輸入12~16位英數字!\n';
            //else if (!isIntEng(obj1.value)) strMsg += '舊密碼請輸入正確12~16位英數字!\n';

            if (isBlank(obj2)) strMsg += '請輸入新密碼!\n';
            else if (obj2.value.length < 12 || obj2.value.length > 16) strMsg += '新密碼長度應為12~16碼，且需至少為英數字混合!\n';
                //else if (!isIntEng(obj2.value)) strMsg += '新密碼請輸入正確12~16位英數字!\n';
            else if (!chkPassFmt(obj2.value)) strMsg += '新密碼長度應為12~16碼，且需至少為英數字混合!\n';

            if (isBlank(obj3)) strMsg += '請輸入確認新密碼!\n';
            //else if (obj3.value.length < 12 || obj3.value.length > 16) strMsg += '確認新密碼請輸入12~16位英數字!\n';
            //else if (!isIntEng(obj3.value)) strMsg += '確認新密碼請輸入正確12~16位英數字!\n';

            if (obj2.value != obj3.value) strMsg += '新密碼與確認新密碼輸入的內容不同!\n';
            break;
        case 'auth': //判斷帳號4~16英數字
            if (isBlank(obj1)) strMsg += '請輸入' + strName + '!\n';
            else if (obj1.value.length < 4 || obj1.value.length > 16) strMsg += strName + '請輸入4~16位英數字!\n';
            else if (!isIntEng(obj1.value)) strMsg += strName + '請輸入正確4~16位英數字!\n';
            break;
        case 'pass': //判斷帳號12~16英數字
            if (isBlank(obj1)) strMsg += '請輸入' + strName + '!\n';
            else if (obj1.value.length < 12 || obj1.value.length > 16) strMsg += strName + '長度應為12~16碼，且需至少為英數字混合!\n';
                //else if (!isIntEng(obj1.value)) strMsg += strName + '請輸入正確12~16位英數字!\n';
            else if (!chkPassFmt(obj1.value)) strMsg += strName + '長度應為12~16碼，且需至少為英數字混合!\n';
            break;
        case 'mail': //判斷電子郵件格式
            if (isBlank(obj1)) strMsg = '請輸入' + strName + '!\n';
            else if (obj1.value.indexOf('@') < 0 || obj1.value.split('@').length != 2) strMsg = strName + '格式有誤!\n';
            break;
        case 'idno':
            if (!checkNID(obj1.value)) strMsg = strName + '格式有誤!\n';
            break;
    }

    return strMsg;
}

/* 依使用者瀏覽器環境設定物件顯示語法 */
function ctrlShowObj(obj) {
    if (navigator.userAgent.toLowerCase().indexOf('msie') > 0) {
        obj.style.display = 'inline';
    } else {
        obj.style.display = 'table-row';
    }
}

/* 檢查指定的text或textarea或hidden表單欄位物件是否已輸入值 */
function isBlank(obj) {
    return isSpace(obj.value);
}

/* 檢查指定的字串是否為空字串或為空白字串 */
function isSpace(value) {
    var result = false;
    if (trim(value) == '') result = true;
    return result;
}

/* 判斷是否為英文字 */
function isEng(value) {
    var pattern = /^[A-Za-z]+$/;
    return pattern.test(value);
}

/* 判斷是否為英文或數字 */
function isIntEng(value) {
    var pattern = /^[A-Za-z0-9]+$/;
    return pattern.test(value);
}

/* 判斷是否為中文 */
function isRoc(value) {
    var pattern = /([^\x00-\x7F])+$/;
    return pattern.test(value);
}

/* 檢核textbox multiline的欄位輸入長度(nvarchar)
* @return boolean 超過字數(false)
*/
function checkTextLength(obj, maxlength) {
    //var maxlength = new Number(long);

    if (!isBlank(obj) && obj.value.length > maxlength) {
        //obj.value = obj.value.substring(0, maxlength);
        return false;
    } else {
        return true;
    }
}

/*限制textbox multiline的欄位可輸入長度(nvarchar)*/
function limitTextLength(obj, maxlength) {
    if (obj.value.length > maxlength) {
        obj.value = obj.value.substring(0, maxlength);
    }
}

/* 判斷指定的字串是否超過最大長度限制值，若超過則傳回true
* @param   value       欲檢查的字串
* @return  boolean
*/
function checkMaxLen(value, Length) {
    var actualLen = getStrLen(value);

    return (actualLen > Length);
}

/* 判斷指定的字串是否小於最小長度限制值，若小於則傳回true
* @param   value       欲檢查的字串
* @return  boolean
*/
function checkMinLen(value, Length) {
    var actualLen = getStrLen(value);

    return (actualLen < Length);
}

/* 取得字串長度 (一個中文字算二個字元) */
/* myStr(欲檢查的字串), return int(字串長度) */
function getStrLen(myStr) {
    var myLength = 0;

    for (var i = 0; i < myStr.length; i++) {
        myLength++;
        if (myStr.charCodeAt(i) > 127) {
            myLength++;
        }
    }
    return myLength;
}

/* 字串補0 (遞迴)*/
function padLeftZero(str, len) {
    if (str.length >= len) return str;
    else return padLeftZero('0' + str, len);
}

/* 進行特定字串全部取代的方法 (正規表示法) */
function replaceAll(txt, findStr, replaceStr) {
    return txt.replace(new RegExp(findStr, 'g'), replaceStr);
}

/* 判斷是否為正整數或負整數，例如: +25, -33, 77 皆符合條件 */
function isInt(value) {
    var pattern = /^(\+|\-)?\d+$/;
    return pattern.test(value);
}

/* 判斷是否為全為數字，例如: 25, 002 皆符合條件 (有含正號、負號的整數不符合條件) */
function isUnsignedInt(value) {
    var pattern = /^\d+$/;
    return pattern.test(value);
}

/* 判斷是否為正整數，例如: +25, 77 皆符合條件 */
function isPositiveInt(value) {
    var pattern = /^(\+)?\d+$/;
    return pattern.test(value);
}

/* 判斷是否為負整數，例如: -33, -006 皆符合條件 */
function isNegativeInt(value) {
    var pattern = /^\-\d+$/;
    return pattern.test(value);
}

/* 判斷是否為正浮點數或負浮點數，例如: +25.7, -33.7, 77.7 皆符合條件 */
function isFloat(value) {
    var pattern = /^(\+|\-)?\d+\.\d+$/;
    return pattern.test(value);
}

/* 判斷是否為正浮點數，例如: +25.7, 77.7 皆符合條件 */
function isPositiveFloat(value) {
    var pattern = /^(\+)?\d+\.\d+$/;
    return pattern.test(value);
}

/* 判斷是否為負浮點數，例如: -33.7, -006.7 皆符合條件 */
function isNegativeFloat(value) {
    var pattern = /^\-\d+\.\d+$/;
    return pattern.test(value);
}

/* 去除掉指定字串的前、後空白字元 */
function trim(strvalue) {
    ptntrim = /(^\s*)|(\s*$)/g;
    return strvalue.replace(ptntrim, '');
}

/* 將指定的數字四捨五入到指定的小數位數 */
function getRound(number, noOfPlaces) {
    if (isNaN(number)) {
        alert('Please enter a valid number');
        Number = 0;
    }

    val = (Math.round(number * Math.pow(10, noOfPlaces))) / Math.pow(10, noOfPlaces);
    val = val.toString();
    ind = val.indexOf('.');

    if (ind == -1) {
        val = val.toString() + '.';
        for (i = 0; i < noOfPlaces; i++) val = val + '0';
    } else {
        len = val.length;
        x = len - ind - 1;

        if (x < noOfPlaces) {
            for (i = 0; i < (noOfPlaces - x) ; i++) val = val + '0';
        }
    }

    return (val);
}

/* 檢查民國日期 */
function checkRocDate(mydate) {
    if (mydate == '') return true;
    var separator = '/';

    // 將日期格式固定為 YYYY/MM/DD or YYYY-MM-DD (即MM, DD皆為兩位數)
    DateString = formatDate(mydate);

    if (DateString.indexOf('/') != -1) separator = '/';
    else return false;

    var idx = DateString.indexOf(separator);
    var y = DateString.substring(0, idx)  //年

    // 使用者可能輸入西元年份
    if (y.length > 3) return false;
    if (isNaN(y)) return false;

    y = parseInt(y);

    if (y == 0) return false;

    var md = DateString.substring(idx + 1, DateString.length);

    y = y + 1911;  //將民國年轉為西元年

    var ymd = y + separator + md;

    return CheckDatefmt(ymd, separator);
}

/* 檢查西元日期是否正確 (格式可為 YYYY-MM-DD 或 YYYY/MM/DD) */
function checkDate(DateString) {
    var separator = '/';

    if (DateString.indexOf('/') != -1) separator = '/';
    else if (DateString.indexOf('-') != -1) separator = '-';

    return CheckDatefmt(DateString, separator);
}

/* 取得兩個日期之間所相差的天數 */
function getDiffDay(date1, date2) {
    var laterdate, earlierdate;
    var difference;

    earlierdate = Date.parse(date1);
    laterdate = Date.parse(date2);

    difference = laterdate - earlierdate;

    return Math.floor(difference / 1000 / 60 / 60 / 24);
}

/* 將傳入的民國日期 (其格式可為YY/MM/DD或YY-MM-DD)格式化為西元年的日期 */
function getAdDate(DateString) {
    var separator = '/';
    if (DateString.indexOf('/') != -1) {
        separator = '/';
    } else if (DateString.indexOf('-') != -1) {
        separator = '-';
    }
    var idx = DateString.indexOf(separator);
    var y = DateString.substring(0, idx)  //年

    y = parseInt(y);

    if (isNaN(y)) return '';
    var md = DateString.substring(idx + 1, DateString.length);
    y = y + 1911;  //將民國年轉為西元年

    var ymd = y + separator + md;
    return ymd;
}

/* 將傳入的西元年日期 (其格式可為YYYY/MM/DD或YYYY-MM-DD)格式化為民國年日期 */
function getRocDate(DateString) {
    var separator = '/';
    if (DateString.indexOf('/') != -1) {
        separator = '/';
    } else if (DateString.indexOf('-') != -1) {
        separator = '-';
    }

    var idx = DateString.indexOf(separator);
    var y = DateString.substring(0, idx)  //年

    y = parseInt(y);

    if (isNaN(y)) return '';
    var md = DateString.substring(idx + 1, DateString.length);
    y = y - 1911;  //將西元年轉為民國年

    var ymd = y + separator + md;
    return ymd;
}

/* 將指定的日期字串加/減天數 */
function addDateByDay(mydate, day) {
    var lngDate, newdate, result;

    lngDate = Date.parse(mydate);
    lngDate += day * 24 * 60 * 60 * 1000;

    newdate = new Date(lngDate);

    result = newdate.getFullYear() + '/';
    if ((newdate.getMonth() + 1) < 10) result += '0';

    result += (newdate.getMonth() + 1) + '/';

    if (newdate.getDate() < 10) result += '0';

    result += newdate.getDate();

    return result;
}

/* 將指定的日期字串加/減月數 */
function addDateByMonth(mydate, value) {
    var newdate, y, m, d, myvalue;

    newdate = new Date(mydate);
    y = newdate.getFullYear();
    m = (newdate.getMonth() + 1);
    d = newdate.getDate();
    myvalue = parseInt(value, 10);

    m += myvalue;

    if (m > 12) y += 1;

    m %= 12;

    if (m == 0) m = 12;

    result = y + '/';

    if (m < 10) result += '0';

    result += m + '/';

    if (d < 10) result += '0';

    result += d;

    if (!checkDate(result)) result = getLastDay(y + '/' + m + '/1');

    return result;
}

/* 取得指定日期下，此月份的最後一天的日期 */
function getLastDay(mydate) {
    var newdate, y, m, d, lngDate, result;

    newdate = new Date(mydate);
    y = newdate.getFullYear();
    m = newdate.getMonth() + 1;
    d = 1;

    if (m == 12) {
        y++; m = 1;
    } else m++;

    lngDate = Date.parse(y + '/' + m + '/' + d);
    lngDate -= 1000;

    newdate = new Date(lngDate);

    result = newdate.getFullYear() + '/';

    if ((newdate.getMonth() + 1) < 10) result += '0';

    result += (newdate.getMonth() + 1) + '/';

    if (newdate.getDate() < 10) result += '0';

    result += newdate.getDate();

    return result;
}

/* 將日期的「月」、「日」格式化為兩位數的 YYYYY/MM/DD 或 YYYY-MM-DD 格式 */
function formatDate(DateString) {
    var separator = '/';
    var idx;
    var temp;
    var y, m, d;
    if (DateString.indexOf('/') != -1) {
        separator = '/';
    } else if (DateString.indexOf('-') != -1) {
        separator = '-';
    }

    // Year
    temp = DateString;
    idx = temp.indexOf(separator);
    y = temp.substring(0, idx);
    temp = temp.substring(idx + 1);

    // Month
    idx = temp.indexOf(separator);
    m = temp.substring(0, idx);
    if (m.length == 1) {
        m = '0' + m;
    }

    // Day
    d = temp.substring(idx + 1);
    if (d.length == 1) {
        d = '0' + d;
    }

    return (y + separator + m + separator + d);
}

/* 檢查日期是否正確，可自定分隔字元，但仍必須符合日期格式=4位年+分隔+2位月+分隔+2位日期 */
function CheckDatefmt(DateString, chrFmt) {
    if (DateString.length > 10 || DateString.length < 8) return false;
    var y, m, d;
    var idx = DateString.indexOf(chrFmt)
    y = DateString.substring(0, idx)  //年

    DateString = DateString.substring(idx + 1, DateString.length)
    var idx = DateString.indexOf(chrFmt)
    m = DateString.substring(0, idx); //月

    d = DateString.substring(idx + 1, DateString.length);  //日

    if (m.substring(0, 1) == '0') m = m.substring(1, m.length);
    if (d.substring(0, 1) == '0') d = d.substring(1, d.length);

    var CharNum = '0123456789';
    //判別是否皆為數字
    for (var i = 0; i < y.length; i++) {
        var str = y.substring(i, i + 1);
        if (CharNum.indexOf(str) < 0) return false;
    }

    for (var i = 0; i < m.length; i++) {
        var str = m.substring(i, i + 1);
        if (CharNum.indexOf(str) < 0) return false;
    }

    for (var i = 0; i < d.length; i++) {
        var str = d.substring(i, i + 1);
        if (CharNum.indexOf(str) < 0) return false;
    }

    y = parseInt(y);
    m = parseInt(m);
    d = parseInt(d);

    if (isNaN(y)) return false;
    if (isNaN(m)) return false;
    if (isNaN(d)) return false;

    if (y < 1900 || y > 2100) return false;

    if (m < 1 || m > 12) return false;

    if (d < 1 || d > 31) return false;

    var isleap = ((y % 100) && !(y % 4)) || !(y % 400);

    switch (m) {
        case 1:
        case 3:
        case 5:
        case 7:
        case 8:
        case 10:
        case 12:
            return true;
        case 4:
        case 6:
        case 9:
        case 11:
            if (d > 30) return false;
            else return true;
        case 2:

            if (isleap) {
                if (d > 29) {
                    return false;
                } else {
                    return true;
                }
            } else {
                if (parseInt(d) > 28) {
                    return false;
                }
            }
            return true;
        default:
            return false;
    }
}

/* 身分證字號檢查 */
function checkNID(source) {
    var veriResult = '';

    source = source.toUpperCase();

    if (source.length > 0) {
        if (source.length != 10) veriResult += '長度應為10個字元 \n';
        else {
            if ((source.charAt(1) > '2') || (source.charAt(1) < '1')) veriResult += '第二個字元須為1或2 \n';
            if (source.toUpperCase().charCodeAt(0) < 65 || source.toUpperCase().charCodeAt(0) > 90) veriResult += '第一個字元必須是英文字母 \n';
            if (isNaN(source.substr(1, 9))) veriResult += '1-9位要數字喔 \n';
        }

        //身分證字號的檢驗公式
        if (veriResult == '') {
            var id = new Array(10);

            for (var i = 0; i < 10; i++) { id[i] = source.charAt(i) }

            var EngString = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';

            id[0] = EngString.indexOf(id[0]);

            var NumArray = new Array(26);

            NumArray[0] = 1; NumArray[1] = 10; NumArray[2] = 19;
            NumArray[3] = 28; NumArray[4] = 37; NumArray[5] = 46;
            NumArray[6] = 55; NumArray[7] = 64; NumArray[8] = 39;
            NumArray[9] = 73; NumArray[10] = 82; NumArray[11] = 2;
            NumArray[12] = 11; NumArray[13] = 20; NumArray[14] = 48;
            NumArray[15] = 29; NumArray[16] = 38; NumArray[17] = 47;
            NumArray[18] = 56; NumArray[19] = 65; NumArray[20] = 74;
            NumArray[21] = 83; NumArray[22] = 21; NumArray[23] = 3;
            NumArray[24] = 12; NumArray[25] = 30;

            var result = NumArray[id[0]];

            var NumString = '0123456789';

            for (var i = 1; i < 10; i++) {
                id[i] = NumString.indexOf(id[i])
                result += id[i] * (9 - i)
            }

            result += 1 * id[9];

            if (result % 10 == 0) return true;

            else return false;
        } else return false;
    } else return false;
}

/* 統一編號格式檢查 */
function ValidateTaxID(vstrTaxID) {
    var i;
    var a1; var a2; var a3; var a4; var a5;
    var b1; var b2; var b3; var b4; var b5;
    var c1; var c2; var c3; var c4;
    var d1; var d2; var d3; var d4; var d5; var d6; var d7;
    var cd8;

    //判斷長度
    if (vstrTaxID.length != 8) return false;

    //判斷字元
    for (i = 0; i <= 7; i++) {
        if ('0123456789'.indexOf(vstrTaxID.substr(i, 1)) < 0) return false;
    }

    //設定變數
    d1 = parseInt(vstrTaxID.substr(0, 1));
    d2 = parseInt(vstrTaxID.substr(1, 1));
    d3 = parseInt(vstrTaxID.substr(2, 1));
    d4 = parseInt(vstrTaxID.substr(3, 1));
    d5 = parseInt(vstrTaxID.substr(4, 1));
    d6 = parseInt(vstrTaxID.substr(5, 1));
    d7 = parseInt(vstrTaxID.substr(6, 1));
    cd8 = parseInt(vstrTaxID.substr(7, 1));

    c1 = d1;
    c2 = d3;
    c3 = d5;
    c4 = cd8;

    a1 = parseInt((d2 * 2) / 10);
    b1 = parseInt((d2 * 2) % 10);

    a2 = parseInt((d4 * 2) / 10);
    b2 = parseInt((d4 * 2) % 10);

    a3 = parseInt((d6 * 2) / 10);
    b3 = parseInt((d6 * 2) % 10);

    a4 = parseInt((d7 * 4) / 10);
    b4 = parseInt((d7 * 4) % 10);

    a5 = parseInt((a4 + b4) / 10);
    b5 = parseInt((a4 + b4) % 10);

    //計算公式
    if ((a1 + b1 + c1 + a2 + b2 + c2 + a3 + b3 + c3 + a4 + b4 + c4) % 10 == 0) return true;

    if (d7 == 7) {
        if ((a1 + b1 + c1 + a2 + b2 + c2 + a3 + b3 + c3 + a5 + c4) % 10 == 0) return true;
    }

    return false;
}

/* 判斷是否為正整數或負整數，例如: +25, -33, 77 皆符合條件 */
function isInt(value) {
    var pattern = /^(\+|\-)?\d+$/;
    return pattern.test(value);
}

/* 判斷是否為英文字 */
function isEng(value) {
    var pattern = /^[A-Za-z]+$/;
    return pattern.test(value);
}

/*判斷是否含有不可輸入的特殊字元(例:空白)*/
function isSpecChar(value) {
    var pattern = /^\s+$/;
    return pattern.test(value);
}

//檢核密碼格式
function chkPassFmt(strPwd) {
    var strVal = "";
    var blDig = false;
    var blEng = false;
    var blSpec = false;

    //檢核是否含有數字
    for (var i = 0; i < strPwd.length; i++) {
        strVal = strPwd.substr(i, 1);

        if (isInt(strVal)) {
            blDig = true;
            break;
        }
    }

    //檢核是否含有英文
    for (var i = 0; i < strPwd.length; i++) {
        strVal = strPwd.substr(i, 1);

        if (isEng(strVal)) {
            blEng = true;
            break;
        }
    }

    //檢核是否含有不可輸入的特殊字(含空白)
    for (var i = 0; i < strPwd.length; i++) {
        strVal = strPwd.substr(i, 1);

        if (isSpecChar(strVal)) {
            blSpec = true;
            break;
        }
    }

    return (blDig && blEng && !blSpec);
}

//防止輸入空白
function banInputSapce(e) {
    var keynum;

    if (window.event) // IE 
    {
        keynum = e.keyCode
    }
    else if (e.which) // Netscape/Firefox/Opera 
    {
        keynum = e.which
    }

    if (keynum == 32) {
        return false;
    }

    return true;
}