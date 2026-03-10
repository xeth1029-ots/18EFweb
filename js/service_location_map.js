/* OrgMapSchController 專用的 javascript */

var stationData = [];
var defaultCityID;
var defaultStationID;
var isMarkerFocused = false;

//function pageLoad(sender, args) {
//     處理 ajaxToolkit:CascadingDropDown populated 事件
//     以便進行預設 City 的查詢
//    var cddCity = $find("cddAddressCity");
//    if (cddCity != null)
//        cddCity.add_populated(function () {
//            $("select[id*=Address_ddlCity]").val($("input[id*=CityID]").val());
//            var cddArea = $find("cddAddressArea");
//            if (cddArea != null) {
//                cddArea._onParentChange(true);
//            }
//            btnSearchClick();
//        });
//}

$(document).ready(function () {

    stationData = liSDM;
    if (stationData == 'undefined') { stationData = []; }
    console.log(stationData);

    defaultCityID = $("input[id*=hidCityID]").val();
    defaultStationID = $("input[id*=hidStationID]").val();

    if (defaultCityID || defaultStationID) {
        // 有預設縣市或站台, 把頁面往上捲, 以最佳化地圖區塊顯示
        myScrollTop(true);
    }

    //if (!defaultCityID) {
    //    沒有預設 City 時才查詢, 有預設City時在cddAddressCity populated 事件時才查詢
    //    searchStation(buildQuery());
    //    showLocationZone();
    //}

    // binding Map event
    mapEventHandler.zoomend = onMapBoundsChanged;
    mapEventHandler.dragend = onMapBoundsChanged;

    if (defaultStationID) {
        //leafletApi.mapSetting.zoom = 16.6;
    }

    leafletApi.initMap("map", function () {
        console.log("Map ready");
    });

    //縮小畫面使用
    //resizeItem('location_zone');
    mapListRwd.init('.location_zone');

});

//function loadingMask(body) { }

//function removeLoadingMask() { }

function findStationData(stationId) {
    var station = undefined;
    for (var i = 0; i < stationData.length; i++) {
        if (stationData[i].TB_ORGID == stationId) {
            station = stationData[i];
            break;
        }
    }
    return station;
}

// 把頁面往上捲, 以最佳化地圖區塊顯示
function myScrollTop(snapMap) {
    var $body = (window.opera) ? (document.compatMode == "CSS1Compat" ? $('html') : $('body')) : $('html,body');
    var sTop = 120;  // 上方標題處

    if (snapMap) {
        // 貼齊地圖上緣
        if ($(window).width() <= 480) {
            // 手機 RWD 模式, 捲到地圖區塊上緣
            sTop = 380;
        }
        $body.animate({ scrollTop: sTop }, 500);
    }
    else {
        // 直接 GET 開啟 
        if ($(window).width() <= 480) {
            // 手機 RWD 模式, 捲到頁面最上方(避免 browser 保存前次的頁面位置)
            $body.animate({ scrollTop: 0 }, 500);
        }
    }
}

// 通知 onMapBoundsChanged() 不要連動更新右側站台清單
var preventBoundsChangeRefreshList = false;

function onMapBoundsChanged(e) {
    console.log("onMapBoundsChanged: zoom=" + leafletApi.getZoom());
    //console.log(e);

    var geo = leafletApi.getMapBounds();
    console.log(geo);

    if (!isMarkerFocused && !preventBoundsChangeRefreshList) {
        // 更新右側站台列表
        showLocationZone(geo);
    }

    if (defaultStationID) {

        var i_timeout1 = 600;
        setTimeout(function () {
            // 有預設站台開啟頁面時, 初始 onMapBoundsChanged 可能會有2次
            // delay 之後再呼叫 leafletApi.focusMark() 連動 highllight 標示

            var marker = leafletApi.findMarkerByItemId(defaultStationID);
            console.log("defaultStationID: " + defaultStationID);
            console.log(marker);

            defaultStationID = undefined;  // reset defaultStationID

            if (marker != undefined && marker != null) {
                leafletApi.focusMark(marker);
            }
        }, i_timeout1);

    }

    // always reset
    preventBoundsChangeRefreshList = false;
}


/* 取得當前輸入的查詢條件 */
function buildQuery() {
    var query = {};
    query.city = $("select[id*=Form_CTID]").val();
    if (query.city) {
        query.cityName = $("select[id*=Form_CTID] option:selected").html();
    }
    else {
        query.cityName = "";
    }
    query.area = $("select[id*=Form_ZIPCODE]").val();
    if (query.area) {
        query.areaName = $("select[id*=Form_ZIPCODE] option:selected").html();
    }
    else {
        query.areaName = "";
    }
    //query.road = $("input[id*=Address_txtDetailAddress]").val();
    query.road = "";
    query.type1 = $("input[id*=Form_IsBranchOffice]").is(":checked");
    query.type2 = $("input[id*=Form_IsTrainingOrg]").is(":checked");
    //query.type3 = $("input[id*=_advType3]").is(":checked");

    console.log("buildQuery:");
    console.log(query);
    return query;
}

/* 
依查詢條件, 查詢過濾服務據點, 符合的資料儲存在 stationData 中
query : 
{
   city: 縣市代碼,
   area: 鄉鎮市區代碼,
   type1: 就業中心(站),
   type2: 就業服務台,
   type3: 分署、青年職涯發展中心、銀髮人才資源中心
}

env: 地圖顯示區域 TGEnvelope
*/
function searchStation(query, geo) {

    for (var i = 0; i < stationData.length; i++) {
        var station = stationData[i];
        if (typeof geo != 'undefined') {
            // 有給定顯示區域(TGrectelope 物件), 依座標判斷是否顯示
            // TGrectelope 物件 Y 座標, 愈上面愈大
            station.show = (
                geo.left <= station.MAP_X
                && geo.right >= station.MAP_X
                && geo.top >= station.MAP_Y
                && geo.bottom <= station.MAP_Y
                )
                & typeMatch(station, query);
        }
        else {
            // 依查詢條件判斷
            station.show = isMatch(station, query);
        }
    }

    function isMatch(station, query) {
        var match = false;
        if (query.city || query.area) {
            if ((!query.area && query.city == station.TB_CTID)
                    || query.area == station.TB_ZIPCODE) {
                match = true;
            }
        }
        else {
            match = true;
        }

        return match & typeMatch(station, query);
    }

    function typeMatch(station, query) {
        var match = false;
        if ((query.type1 && station.TB_TYPE == "1")
                    || (query.type2 && station.TB_TYPE == "2")) {
            match = true;
        };
        return match;
    }
}

function showStationMarkers() {
    console.log("showStationMarkers");

    // 檢查並移除舊標記點
    //clearMarkers();

    // 標點顏色
    var colors = ["#d64161", "#ff7b25", "#30ae30"];

    // 準備標記點資料
    var markerDatas = [];
    var defaultStationData = undefined;
    for (var i = 0; i < stationData.length; i++) {
        var station = stationData[i];

        if (!station.show) {
            // 不顯示, 略過
            continue;
        }

        var locX = parseFloat(station.MAP_X);
        var locY = parseFloat(station.MAP_Y);
        if (isNaN(locX) || isNaN(locY)) {
            //沒有座標點, 略過
            continue;
        }

        var markerData = {};
        markerData.id = station.TB_ORGID;
        markerData.name = station.TB_ORGNAME;
        //markerData.markerImgUrl = getImgUrl(station.STATION_TYPE);
        markerData.class = "custom-cluster-" + station.TB_TYPE;
        markerData.locX = locX;
        markerData.locY = locY;
        //markerData.onClick = stationMarkClick;
        //markerData.data = station;
        markerDatas.push(markerData);

        if (defaultStationID && defaultStationID == markerData.id) {
            defaultStationData = markerData;
        }
    }

    leafletApi.setMarkers(markerDatas, onMarkerClick);

    if (defaultStationData != undefined) {
        // 預設站台點位 focus 顯示
        leafletApi.setCenter({ lat: defaultStationData.locY, lng: defaultStationData.locX }, 12.5);
    }
}

// 地圖 cluster 點位 click 事件處理
function onMarkerClick(m) {
    console.log("onMarkerClick");
    console.log(m);

    isMarkerFocused = true;

    // 重新過濾右側站台清單, 只顯示點選 marker 包含的站台 
    for (var i = 0; i < stationData.length; i++) {
        var station = stationData[i];
        var isShow = false;
        for (var k = 0; k < m.length; k++) {
            if (m[k].itemId == station.TB_ORGID) {
                isShow = true;
                break;
            }
        }
        station.show = isShow;
    }
    showLocationZone();

    // 行動裝置環境中, 重設列表清單至第1個位置
    mapListRwd.resetList();
}


function showLocationZone(geo) {
    let itemTemplete = $("#location_item_templete").html();
    let html = "";
    let showCount = 0;
    let listZone = $(".location_zone");

    // 清除列表
    listZone.find("ul").each(function () {
        $(this).hide('', function () { $(this).remove(); });
    });


    let bounds = {};
    bounds.left = Number.MAX_VALUE;
    bounds.right = 1;
    bounds.bottom = Number.MAX_VALUE;
    bounds.top = 1;

    for (var i = 0; i < stationData.length; i++) {
        let station = stationData[i];
        // 站台不顯示或沒有座標點, 略過
        if (!station.show || isNaN(station.MAP_X) || isNaN(station.MAP_Y)) { continue; }

        let locX = parseFloat(station.MAP_X);
        let locY = parseFloat(station.MAP_Y);

        let isShow = true;
        if (geo != undefined) {
            // 根據地圖區域顯示對應站台
            isShow = (geo.left <= locX && geo.right >= locX && geo.bottom <= locY && geo.top >= locY);
        }

        if (!isShow) {
            continue;
        }

        bounds.left = (bounds.left > locX) ? locX : bounds.left;
        bounds.right = (bounds.right < locX) ? locX : bounds.right;
        bounds.bottom = (bounds.bottom > locY) ? locY : bounds.bottom;
        bounds.top = (bounds.top < locY) ? locY : bounds.top;

        let stationItem = itemTemplete;
        stationItem = stationItem.replace(/{TB_ORGID}/g, station.TB_ORGID);
        stationItem = stationItem.replace("{TB_ORGNAME}", station.TB_ORGNAME);
        stationItem = stationItem.replace("{TB_ADDRESS1}", station.TB_ADDRESS1);
        stationItem = stationItem.replace("{TB_PHONE}", station.TB_PHONE);
        let vTBC_CSS = "primary";
        if (station.TB_TYPE == "2") { vTBC_CSS = "secondary"; }
        stationItem = stationItem.replace("{TBC_CSS}", vTBC_CSS);

        $(stationItem)
            .css('display', 'none')
            .appendTo(listZone)
            .show('slow');

        showCount++;
    }

    if (showCount == 0) {
        $("<ul style=\"display:none\"><li class=\"info-title\"><span class=\"msg\">此區域無服務據點</span></li></ul>")
            .appendTo(listZone)
            .show('slow');
    }

    if (showCount > 0) {
        listZone.find("ul")
            .on("click", function () { stationListItemClick($(this).attr("id")); })
            .mouseenter(function () { stationListItemHoverIn($(this).attr("id")); })
            .mouseleave(function () { stationListItemHoverOut($(this).attr("id")); })
        ;
    }

    return { 'count': showCount, 'bounds': bounds };
}

// 右側清單, 最後 highlight 對應的點位 cluster marker 
var curHighlightMarker;

function stationListItemHoverIn(stationId) {
    //console.log("stationListItemHoverIn: " + stationId);

    if (curHighlightMarker != undefined && curHighlightMarker.removeIconClass) {
        curHighlightMarker.removeIconClass("custom-cluster-hint");
        curHighlightMarker = undefined;
    }

    var marker = leafletApi.findMarkerByItemId(stationId);
    if (marker != undefined && marker != null) {
        // 找到點位, 同時在顯示範圍
        curHighlightMarker = marker;
        marker.addIconClass("custom-cluster-hint");
    }
}

function stationListItemHoverOut(stationId) {
    //console.log("stationListItemHoverOut: " + stationId);

    if (curHighlightMarker != undefined && curHighlightMarker.removeIconClass) {
        curHighlightMarker.removeIconClass("custom-cluster-hint");
        curHighlightMarker = undefined;
    }
    else {
        var marker = leafletApi.findMarkerByItemId(stationId);
        if (marker != undefined && marker != null) {
            // 找到點位, 同時在顯示範圍
            marker.removeIconClass("custom-cluster-hint");
        }
    }
}

// 點選右側列表
function stationListItemClick(stationId) {
    for (var i = 0; i < stationData.length; i++) {
        var station = stationData[i];
        if (!station.show || !station.MAP_X || !station.MAP_Y) {
            // 不顯示或沒有座標點, 略過
            continue;
        }
        if (station.TB_ORGID == stationId) {
            // 更新/保存 點選 station id
            $("input[id*=TB_ORGID]").val(stationId);

            var marker = leafletApi.findMarkerByItemId(stationId);
            if (marker != undefined && marker != null) {
                // 找到點位, 同時在顯示範圍

                // 通知 onMapBoundsChanged() 不要連動更新右側站台清單
                preventBoundsChangeRefreshList = true;

                // 加上提示的CSS樣式
                marker.addIconClass("custom-cluster-hint");
                // 將點位移到地圖中央
                leafletApi.setCenter(marker.getLatLng());
            }
            break;
        }
    }
}

/* 交通資訊 按鈕 click */
function stationShowInfoClick(stationId) {
    var stationInfo = "<ul>";
    for (var i = 0; i < stationData.length; i++) {
        var station = stationData[i];
        if (station.TB_ORGID == stationId) {
            stationInfo += "<li><i>" + station.TB_ORGNAME + "</i></li>";
            stationInfo += "<li><ul>"
                + (station.STATION_ADDRESS != '' ? "<li class=\"transfer-header\">地址：</li><li class=\"transfer-body\">" + station.TB_ADDRESS1 + "</li>" : '')
                + (station.STATION_TEL1 != '' ? "<li class=\"transfer-header\">電話：</li><li class=\"transfer-body\">" + station.TB_PHONE + "</li>" : '')
                + "</ul></li>";
            break;
        }
    }
    stationInfo += "</ul>";

    $("#modal_body_content").html(stationInfo);
    $("#myModal_value").modal("show");
}

function btnLocateClick() {
    // 按定位時, 清除預設站台, 不然預設站台訊息窗會取代使用所在點位視息窗
    $("input[id*=TB_ORGID]").val("");

    var userZoom = 12;

    // 地圖加上 loading mask
    loadingMask("body");

    locateUserPoint(
        function (p) {
            leafletApi.setCenter(p, userZoom);
            leafletApi.setSingleMarker(p, 40, "您所在的位置");
            removeLoadingMask();
        },
        function (msg) {
            removeLoadingMask();
            alert("定位失敗: \n" + msg);
        }
    );
}

function btnSearchClick(geo) {

    isMarkerFocused = false;

    // 全頁面加上 loading mask
    loadingMask("body");

    //console.log("btnSearchClick");
    //console.log(event);
    if (event && event.target) {
        var targetId = $(event.target).attr("id");
        if (targetId == "btnBoundSearch" || targetId == "btnQuery") {
            // 按查詢時, 清除預設站台
            $("input[id*=TB_ORGID]").val("");
        }
    }

    myScrollTop(true);

    var query = buildQuery();
    var dist = query.cityName + query.areaName;
    var road = query.road;

    if (geo) {
        // 根據地圖顯示區域搜尋
        searchStation(query, geo);
        showStationResult();
        removeLoadingMask();
    }
    else {
        if (!road) {
            // 縣市搜尋
            searchStation(query);
            showStationResult(true, !query.cityName);
            removeLoadingMask();
        }
        else {
            // 20210201, 這一段已不會再進來
            // 地圖 定位/地標 搜尋
            var dialog = $("#addressSelModal");
            locateSearch('', dist, road, dialog, function (reqStr, geo, loc, geometry) {
                console.log("locateSearch finish: " + reqStr);

                searchStation(query, geo);
                showStationResult();

                // 更新地圖顯示區域及搜尋點位標記
                leafletApi.setSingleMarker(loc, 40, reqStr);
                leafletApi.setMapBounds(geo);

                removeLoadingMask();
            },
            function (msg) {
                removeLoadingMask();
                alert(msg);
            });
        }
    }

    function showStationResult(fitBounds, showAll) {
        var results = showLocationZone();  // 更新右側站台清單

        if (!defaultStationID && fitBounds && results.count > 0) {  // 依查詢結果, 顯示全部點位範圍

            if (showAll) {
                // 顯示全部站台時, 稍微放大顯示範圍, 連江就服中心點位才會看的到
                results.bounds.top += 0.2;
                results.bounds.bottom -= 0.05;
            }

            leafletApi.setMapBounds(results.bounds);
        }

        showStationMarkers(); // 更新點位
    }
}



function btnBoundSearchClick() {
    // 地圖顯示區域的 TGEnvelope
    var geo = leafletApi.getMapBounds();
    btnSearchClick(geo);
}