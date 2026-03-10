/* service_location.aspx 專用的 javascript */

var stationData = [];

$(document).ready(function () {

    stationData = liSD;
    if (stationData == 'undefined') {
        stationData = [];
    }
    console.log(stationData);
    ////點選第一層標頭第二層縣市內容全部隱藏
    //$(".accordionTitle").on('click', function () {
    //    $('.submenu .panel-collapse.collapse').collapse('hide');
    //});
});

/* 檢視明細 click */
function stationShowInfoClick(stationId) {
    var stationInfoTitle = "";
    //var stationInfo = "<div class=\"location-info-content\">";
    //for (var i = 0; i < stationData.length; i++) {
    //    var station = stationData[i];
    //    if (station.STATION_ID == stationId) {
    //        stationInfoTitle += (station.STATION_NAME != '' ? station.STATION_NAME + "明細" : "");
    //        stationInfo += "<div>" + station.STATION_NAME + "</div>";
    //        stationInfo += "<div>業務及服務項目：<div>" + station.COMMENT + "</div></div>";
    //        stationInfo += "<div class=\"row\"><div class=\"col-md-6 col-xs-12\">電話：" + station.TEL1 + "</div><div class=\"col-md-6 col-xs-12\">傳真：" + station.FAX + "</div></div>";
    //        stationInfo += "<div>地址：" + station.ADDRESS + "</div>";
    //        stationInfo += "<div><div>交通資訊：</div>"
    //            + (station.INFO_MRT != '' ? "<div>捷運族：" + station.INFO_MRT + "</div>" : '')
    //            + (station.INFO_BUS != '' ? "<div>公車族：" + station.INFO_BUS + "</div>" : '')
    //            + (station.INFO_TRAIN != '' ? "<div>火車族：" + station.INFO_TRAIN + "</div>" : '')
    //            + (station.INFO_DRIVE != '' ? "<div>開車族：" + station.INFO_DRIVE + "</div>" : '')
    //            + (station.INFO_SCOOTER != '' ? "<div>機車族：" + station.INFO_SCOOTER + "</div>" : '')
    //            + (station.INFO_WALK != '' ? "<div>走路族：" + station.INFO_WALK + "</div>" : '')
    //            + (station.INFO_OTHER != '' ? "<div>其他：" + station.INFO_OTHER + "</div>" : '')
    //            + (station.INFO_LANDMARK != '' ? "<div>明顯地標：" + station.INFO_LANDMARK + "</div>" : '')
    //            + "</div>";
    //        stationInfo += "<div>備註：<div>" + station.MEMO + "</div></div>";
    //        break;
    //    }
    //}
    //stationInfo += "</div>";

    var stationInfo = "<ul>";
    for (var i = 0; i < stationData.length; i++) {
        var station = stationData[i];
        if (station.STATION_ID == stationId) {
            stationInfoTitle += (station.STATION_NAME != '' ? station.STATION_NAME + "明細" : "");
            stationInfo += "<li><i>" + station.STATION_NAME + "</i></li>";
            stationInfo += "<li><ul>"
                + (station.COMMENT != '' ? "<li class=\"transfer-header\">業務及服務項目：</li><li class=\"transfer-body\">" + station.COMMENT + "</li>" : '')
                + (station.TEL1 != '' ? "<li class=\"transfer-header\">電話：</li><li class=\"transfer-body\">" + station.TEL1 + "</li>" : '')
                + (station.FAX != '' ? "<li class=\"transfer-header\">傳真：</li><li class=\"transfer-body\">" + station.FAX + "</li>" : '')
                + (station.ADDRESS != '' ? "<li class=\"transfer-header\">地址：</li><li class=\"transfer-body\">" + station.ADDRESS + "</li>" : '')
                + (station.INFO_MRT != '' ? "<li class=\"transfer-header\">捷運族：</li><li class=\"transfer-body\">" + station.INFO_MRT + "</li>" : '')
                + (station.INFO_BUS != '' ? "<li class=\"transfer-header\">公車族：</li><li class=\"transfer-body\">" + station.INFO_BUS + "</li>" : '')
                + (station.INFO_TRAIN != '' ? "<li class=\"transfer-header\">火車族：</li><li class=\"transfer-body\">" + station.INFO_TRAIN + "</li>" : '')
                + (station.INFO_DRIVE != '' ? "<li class=\"transfer-header\">開車族：</li><li class=\"transfer-body\">" + station.INFO_DRIVE + "</li>" : '')
                + (station.INFO_SCOOTER != '' ? "<li class=\"transfer-header\">機車族：</li><li class=\"transfer-body\">" + station.INFO_SCOOTER + "</li>" : '')
                + (station.INFO_WALK != '' ? "<li class=\"transfer-header\">走路族：</li><li class=\"transfer-body\">" + station.INFO_WALK + "</li>" : '')
                + (station.INFO_OTHER != '' ? "<li class=\"transfer-header\">其他：</li><li class=\"transfer-body\">" + station.INFO_OTHER + "</li>" : '')
                + (station.INFO_LANDMARK != '' ? "<li class=\"transfer-header\">明顯地標：</li><li class=\"transfer-body\">" + station.INFO_LANDMARK + "</li>" : '')
                + (station.MEMO != '' ? "<li class=\"transfer-header\">備註：</li><li class=\"transfer-body\">" + station.MEMO + "</li>" : '')
                + "</ul></li>";
            break;
        }
    }
    stationInfo += "</ul>";
    //console.log(stationInfo);

    $("#modal_title").html(stationInfoTitle);
    $("#modal_body_content").html(stationInfo);
    $("#myModal_value").modal("show");
}