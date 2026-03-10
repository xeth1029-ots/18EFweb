/* 
  服務據點地圖 及 地圖找工作 在 RWD 480px 模式中，
  右方列表顯示及行動裝置 swipe 事件處理
*/

Object(
    mapListRwd = {

        ww: 0,
        deviceWidthThreshold: 480,  // 行動裝置辨識寛度
        targetSelector: undefined,
        targetElement: undefined,
        listItem: [],  // 列表中的 ul 項目清單
        shiftCount:0,   // 列表項目 swipe left 的次數

        init: function(selector){
            this.targetSelector = selector;
            this.targetElement = $(selector);
            if (this.targetElement.length == 0) {
                console.error("mapListRwd.init: target element '" + selector + "' Not Fund ");
                return;
            }

            this.listItem = this.targetElement.find("ul");

            $(window).resize(function () {
                mapListRwd.ww = $(window).width();
            });

            // binding swipe event
            this.targetElement.swipe({
                // Generic swipe handler for all directions
                swipe: function (event, direction, distance, duration, fingerCount, fingerData) {

                    if (mapListRwd.listItem.length == 0) {
                        // try get mapListRwd.listItem again
                        mapListRwd.listItem = mapListRwd.targetElement.find("ul");
                    }
                    mapListRwd.listCount = mapListRwd.listItem.filter(':visible').length;

                    if ($(window).width() <= mapListRwd.deviceWidthThreshold) {
                        // mobile device
                        console.log(mapListRwd.targetSelector + " swiped " + direction + ", " + mapListRwd.shiftCount + "/" + mapListRwd.listCount);

                        if (direction == "right") {

                            if (mapListRwd.shiftCount == 0) {
                                return;
                            }

                            mapListRwd.shiftCount--;
                        }
                        else if (direction == "left") {

                            if (mapListRwd.shiftCount >= (mapListRwd.listCount - 1) ) {
                                return;
                            }

                            mapListRwd.shiftCount++;
                        }
                        // 根據 shiftCount 決定 list 內容的向左移的距離: margin-left: -[xxx]px
                        var marginLeft = ($(this).width() / mapListRwd.listCount) * mapListRwd.shiftCount;
                        $(this).animate({ 'margin-left': "-" + marginLeft + "px" });
                    }

                }
            });

        },

        // 行動裝置環境中, 重設列表清單至第1個位置
        resetList: function () {
            mapListRwd.listItem = mapListRwd.targetElement.find("ul");

            if ($(window).width() <= mapListRwd.deviceWidthThreshold) {
                mapListRwd.shiftCount = 0;
                this.targetElement.animate({ 'margin-left': "0px" });
            }
        }
    }      
);



/*
function resizeItem(Selector) {
    ww = $(window).width();
    ul = document.querySelector('.' + Selector);
    li = document.querySelectorAll('.' + Selector + ' ul');
    if (ww <= 480) {
        move = 0;
        count = 0;
        ul.style.marginLeft = move;
        console.log(Log("ww <= 480"));
        if ("addEventListener" in window) {
            ul.addEventListener("mousewheel", mouse_wheel, false);
            ul.addEventListener("DOMMouseScroll", mouse_wheel, false);
        }
    }
    else {
        console.log(Log("ww > 480"));
        ul.removeEventListener("mousewheel", mouse_wheel, false);
        ul.removeEventListener("DOMMouseScroll", mouse_wheel, false);
    }
}


var ww, ul, li, move, count;
//google 找到的滑鼠滾輪事件
function mouse_wheel(e) {
    e = e || window.event;
    e.preventDefault();
    if (e.wheelDelta <= 0 || e.detail > 0) {
        if (count > (li.length - 2)) { return }
        move = move - (ul.offsetWidth / li.length);

        if (move < (0 - ul.offsetWidth)) {
            ul.style.marginLeft = (0 - ul.offsetWidth) + "px";
        }
        else {
            ul.animate( { 'margin-left':  move + "px" } );
        }
        count++;
    }
    else {
        count--;
        if (count < 0) { count = 0 };
        if (parseInt(ul.style.marginLeft) == 0) { return }
        move = move + (ul.offsetWidth / li.length);
        if (move > 0) { ul.style.marginLeft = "0px"; }
        else {
            ul.style.marginLeft = move + "px";
        }
    }
}
*/

