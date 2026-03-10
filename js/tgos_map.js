/*tgos map api*/

function Log(msg) {
    if (typeof msg == "string") {
        var now = new Date();
        var t = "[" + now.getMinutes() + ":" + now.getSeconds() + "." + now.getMilliseconds() + "] ";
        msg = t + msg;
    }
    return msg;
}

/* 
依輸入的地標名稱,縣市區域或路名定位,
查詢成功能, callback 呼叫: 
onSuccess(formattedAddress, env, loc, geometry) 
其中:
  env: 系統建議的視域範圍(TGEnvelope)
  loc: geometry.location取得點位(TGPoint)
*/
function locateSearch(poi, dist, roadOrAddr, modalDialog, onSuccess, onFailed, poiDisabled, ignoreManyResult) {
    poi = poi ? poi.trim() : "";
    dist = dist ? dist.trim() : "";
    roadOrAddr = roadOrAddr ? roadOrAddr.trim() : "";

    if(!poi && !dist && !roadOrAddr) {
        return;
    }

    var locator = new TGOS.TGLocateService();  //宣告定位物件
    var req = {};
    var reqStr = "";

    if (poi) {
        // 地標定位
        req.poi = poi;
        reqStr = poi;
    }
    else if(roadOrAddr) {
        // 道路名稱或地址 定位
        if (ignoreManyResult) {
            // 一律視為 地址定位
            req.address = roadOrAddr;
        }
        else {
            req.roadLocation = (dist) ? (dist + roadOrAddr) : roadOrAddr;
        }

        reqStr = req.roadLocation;
    }
    else {
        // 行政區定位
        req.district = dist;
        reqStr = dist;
    }

    console.log(Log("locateWGS84: "));
    console.log(Log(req));

    //進行定位查詢, 並指定回傳資訊為WGS84坐標系統
    locator.locateWGS84(req, function (result, status) {
        console.log( Log(status) );
        console.log( Log(result) );

        if ((status != TGOS.TGLocatorStatus.OK && status != TGOS.TGLocatorStatus.TOO_MANY_RESULTS)
            || result == null || result.length == 0) {

            if (req.district && !roadOrAddr && !poiDisabled) {
                // 行政區定位無資料, 試著再以地標定位
                var cPoi = req.district;
                locateSearch(cPoi, '', '', modalDialog, onSuccess, onFailed);
            }
            else if ((req.district || req.roadLocation) && roadOrAddr && !poiDisabled) {
                // 行政區域或道路定位無資料, 試著再以地標定位
                var cPoi = roadOrAddr;
                locateSearch(cPoi, '', '', modalDialog, onSuccess, onFailed);
            }
            else {
                var msg = '查無指定的地圖區域!';
                if (typeof onFailed == 'function') {
                    onFailed(msg);
                }
                else {
                    alert(msg);
                }
            }
            return;
        }

        var resultFiltered = [];
        if (status == TGOS.TGLocatorStatus.TOO_MANY_RESULTS) {
            // 有多個結果, 先去掉 巷 的地址項目(不用到那麼細)
            for (var i = 0; i < result.length; i++) {
                var item = result[i];

                if (item.formattedAddress.indexOf("巷") > -1) {
                    // 不用顯示包含 巷 的地址項目(不用到那麼細)
                    continue;
                }
                resultFiltered.push(item);
            }
            result = resultFiltered;
        }
        
        if (status == TGOS.TGLocatorStatus.TOO_MANY_RESULTS && result.length > 1) {
            
            if (ignoreManyResult) {
                // 有多個結果, 但只回傳第1筆定位資料
                //以geometry.viewport取得系統建議的視域範圍(TGEnvelope)
                var env = result[0].geometry.viewport;
                //利用geometry.location取得點位(TGPoint)
                var loc = result[0].geometry.location;
                //行政區域空間資訊
                var geometry = result[0].geometry.geometry;
                // 定位回傳的地址
                var county = result[0].county ? result[0].county : "";
                var town = result[0].town ? result[0].town : "";
                var address = result[0].poiName ? result[0].poiName : (county + town + result[0].formattedAddress);

                /*
                if (!address) {
                    // 回傳結果中沒有定位點資訊, 以查詢詞回饋
                    address = reqStr;
                }
                */

                loc = [loc.y, loc.x];
                console.log(Log(loc));

                if (typeof onSuccess == 'function') {
                    // 單一結果時, 一律以查詢詞回傳, 以避免 TGOS 回應的點位名稱太奇怪
                    onSuccess(reqStr, env, loc, geometry, false);
                }
            }
            else {
                // 有多個結果, 顯示選單供選擇
                hideLoadingMask();  // 隱藏 loadingMask

                showAddressSelectDialog(modalDialog, result, function (address, bounds, location) {

                    if (!poi) {
                        // 不是地標定位時
                        // TGOS 在多個結果時, bounds 物件會包含所有可能的範圍
                        // 若直接選定的項目的 bounds 去顯示地圖, 範圍會不正確
                        // 要再次以具體地址去 locate 一次
                        console.log(Log("showAddressSelectDialog callback: " + address));

                        showLoadingMask();  // 再次開啟 loadingMask

                        if (roadOrAddr) {
                            locateSearch('', '', address, modalDialog, onSuccess, onFailed);
                        }
                        else {
                            locateSearch('', address, '', modalDialog, onSuccess, onFailed);
                        }
                    }
                    else {
                        // 地標定位, 多個結果, 個別 bounds 物件只會包含所在的範圍, 不用重新查詢
                        // 將 bounds 物件轉成 TGEnvelope
                        var env = new TGOS.TGEnvelope();
                        env.setLeft(bounds.left);
                        env.setTop(bounds.top);
                        env.setRight(bounds.right);
                        env.setBottom(bounds.bottom);
                        console.log(Log("showAddressSelectDialog callback: " + address));
                        console.log(Log(env));

                        var loc = [location.y, location.x];
                        console.log(Log(loc));

                        // 定位結果顯示
                        if (typeof onSuccess == 'function') {
                            onSuccess(address, env, loc, geometry, true);
                        }
                    }
                });
            }

        }
        else if(status == TGOS.TGLocatorStatus.OK) {
            // 單一結果
            //以geometry.viewport取得系統建議的視域範圍(TGEnvelope)
            var env = result[0].geometry.viewport;
            //利用geometry.location取得點位(TGPoint)
            var loc = result[0].geometry.location;
            //行政區域空間資訊
            var geometry = result[0].geometry.geometry;
            // 定位回傳的地址
            var county = result[0].county ? result[0].county : "";
            var town = result[0].town ? result[0].town : "";
            var address = result[0].poiName ? result[0].poiName : (county + town + result[0].formattedAddress);

            /*
            if (!address) {
                // 回傳結果中沒有定位點資訊, 以查詢詞回饋
                address = reqStr;
            }
            */

            loc = [loc.y, loc.x];
            console.log(Log(loc));

            if (typeof onSuccess == 'function') {
                // 單一結果時, 一律以查詢詞回傳, 以避免 TGOS 回應的點位名稱太奇怪
                onSuccess(reqStr, env, loc, geometry, false);
            }
        }
        else {
            var msg = '查無指定的地圖區域!';
            if (typeof onFailed == 'function') {
                onFailed(msg);
            }
        }

    });
}


/* 
LocateSearch 多個結果時的動態 modal dialog 選單
target 必須是已存在的 bootstrap Modal
*/
function showAddressSelectDialog(target, resultData, callback) {

    console.log( Log(target) );
    if (!target || target == 'undefined' || target.length == 0) {
        alert("showAddressSelectDialog: target Modal dialog Not defined.");
    }

    if (resultData) {
        // TGOS.TGLocateService.locateTWD97() 回傳的 result 資料
        console.log( Log(resultData) );

        var modalBody = target.find(".modal-body");
        var selContent = "<ul class='addressSelect'>";
        var showItem = 10;  // 最多顯示10筆
        for (var i = 0; i < resultData.length && showItem > 0; i++, showItem--) {
            var item = resultData[i];

            if (item.formattedAddress.indexOf("巷") > -1) {
                // 不用顯示包含 巷 的地址項目(不用到那麼細)
                continue;
            }

            var county = item.county ? item.county : "";
            var town = item.town ? item.town : "";

            var title = item.poiName ? (county + town + item.poiName) : (county + town + item.formattedAddress);

            var geometry = item.geometry;            
            selContent += "<li data-left='" + geometry.viewport.left + "' data-right='" + geometry.viewport.right + "' data-top='" + geometry.viewport.top + "' data-bottom='" + geometry.viewport.bottom + "' data-x='" + geometry.location.x + "' data-y='" + geometry.location.y + "'>" + title + "</li>";
        }

        selContent += "</ul>";
        modalBody.html(selContent);
    }

    modalBody.find("li").on("click", function () {
        target.modal('hide');

        console.log( Log(this) );
        var address = $(this).text();
        var bounds = {};
        var location = {};

        bounds.left = $.parseJSON($(this).data("left"));
        bounds.right = $.parseJSON($(this).data("right"));
        bounds.top = $.parseJSON($(this).data("top"));
        bounds.bottom = $.parseJSON($(this).data("bottom"));
        location.x = $.parseJSON($(this).data("x"));
        location.y = $.parseJSON($(this).data("y"));

        callback(address, bounds, location);
    });

    target.modal({ show: true });
}

/* 
定位使用者所在座標, 
成功會呼叫 onSuccess([WGS84_Y, WGS84_X]),
失敗會呼叫 onError(errCode, errMessage)
*/
function locateUserPoint(onSuccess, onError) {

    //GPS定位
    navigator.geolocation.getCurrentPosition(success, error);

    function success(pos) {
        var crd = pos.coords;

        console.log( Log('Your current position is:') );
        console.log( Log('Latitude(Y): ' + crd.latitude) );
        console.log( Log('Longitude(X): ' + crd.longitude) );
        console.log( Log('More or less ' + crd.accuracy + ' meters.') );

        onSuccess([crd.latitude, crd.longitude]);
    };

    function error(err) {
        console.warn('getCurrentPosition ERROR(' + err.code + '): ' + err.message);

        onError(err.code, err.message);
    };

}
