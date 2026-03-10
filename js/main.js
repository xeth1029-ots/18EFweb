
//yo - loadiing icon
// 顯示讀取遮罩
function ShowProgressBar() {
    
    displayProgress();
    displayMaskFrame();
}

// 隱藏讀取遮罩
function HideProgressBar() {
    var progress = $('#divProgress');
    var maskFrame = $("#divMaskFrame");
    progress.hide();
    maskFrame.hide();
}
// 顯示讀取畫面
function displayProgress() {
    var w = $(document).width();
    var h = $(window).height();
    var progress = $('#divProgress');
    progress.css({ "z-index": 999999, "top": (h / 2) - (progress.height() / 2), "left": (w / 2) - (progress.width() / 2) });
    progress.show();
}
// 顯示遮罩畫面
function displayMaskFrame() {
    var w = $(window).width();
    var h = $(document).height();
    var maskFrame = $("#divMaskFrame");
    maskFrame.css({ "z-index": 999998, "opacity": 0.7, "width": w, "height": h });
    maskFrame.show();
}
//
//資安設定
//if (self == top) { document.documentElement.style.display = 'block'; }else { top.location = self.location; }
//if (parent.document.frames != undefined && parent.document.frames.length != 0) {
//    top.location.replace(self.location);
//}

$(document).ready(function () {
    //版面調整
    changeHeightByHeader();
    $(window).resize(function () {
        this.changeHeightByHeader();
    });
    /*Structure.ascx*/
    $('#collapseStructure').on('shown.bs.collapse', function () {
        $(this).prev().find(".glyphicon").removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
    });

    //The reverse of the above on hidden event:

    $('#collapseStructure').on('hidden.bs.collapse', function () {
        $(this).prev().find(".glyphicon").removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
    });
});
function changeHeightByHeader() {
    $('.container:first').css("margin-top", $("#header_bar").height());
}
/*Menu.ascx*/
function showSubMenu(el) {
    $('#menu').find('ul[role="menuitem"]').css('visibility', 'hidden');  // 先隱藏全部選單
    var uls = $(el).closest('li').find('ul');
    if (uls && uls.find('a').length > 0) {
        uls.css('visibility', 'visible');    // 顯示選取選項的子選單
    } else {
        var url = $(el).attr('data-link');
        var target = $(el).attr('target');
        window.open(url, target);
    }
}
$(document).on('click', ".showSubMenu", function (e) {
    var visible = $(e.currentTarget)
        .closest('li[role=menu]')
        .find('ul[role="menuitem"]')
        .css('visibility');  // 取得目前子選單狀態 

    if (visible) {
        if (visible == 'hidden') {
            showSubMenu(this);
        } else if (visible == 'visible') {
            $(e.currentTarget)
                .closest('li[role=menu]')
                .find('ul[role="menuitem"]')
                .css('visibility', 'hidden');  // 先隱藏全部選單
        }
    } else {
        showSubMenu(this);
    }
});

/*footer.ascx*/
$(function () {
    $("#contant").click(function () {
        $.blockUI({ message: $("#block_contantus"), css: { width: "400px",'z-index':1001 } })
        document.body.onkeydown = function (e) {
            // alert(String.fromCharCode(e.keyCode)+" --> " + e.keyCode);
            var keyCode = e.keyCode;
            if (keyCode && keyCode == 27) { // 27:ESC鍵
                $.unblockUI();
                document.body.onkeydown = null;  // 關閉視窗後取消事件綁定
            }
        };
    });
    $("#closeUI_contantus").click(function () { $.unblockUI(); return false });
});

/* UC_sidebar01.ascx */
$(document).ready(function () {
    function focusFirst(el) {
        $(el).find('a:first').focus();
    }

    function click01() {
        $('#tab01').parent().addClass("active");
        $('#tab02').parent().removeClass("active");
        $('#tab03').parent().removeClass("active");

        $("#pan01").addClass("active");
        $("#pan02").removeClass("active");
        $("#pan03").removeClass("active");

        focusFirst('#pan01');
    }

    function click02() {
        $('#tab01').parent().removeClass("active");
        $('#tab02').parent().addClass("active");
        $('#tab03').parent().removeClass("active");

        $("#pan01").removeClass("active");
        $("#pan02").addClass("active");
        $("#pan03").removeClass("active");

        focusFirst('#pan02');
    }

    function click03() {
        $('#tab01').parent().removeClass("active");
        $('#tab02').parent().removeClass("active");
        $('#tab03').parent().addClass("active");

        $("#pan01").removeClass("active");
        $("#pan02").removeClass("active");
        $("#pan03").addClass("active");

        focusFirst('#pan03');
    }

    $('#tab01').on('click', click01);
    $('#tab02').on('click', click02);
    $('#tab03').on('click', click03);
});

/* 熱門活動: UC_sideber02.ascx */
$(document).ready(function () {
    (function (root) {
        var self = this;
        var isFocus = false;

        self.prev = function () {
            $('#slideshow > div:last').prependTo('#slideshow');
            $('#slideshow > div').fadeOut(0);
            $('#slideshow > div:first').fadeIn(0);
        };

        self.next = function () {
            $('#slideshow > div:first')
                .fadeOut(0)
                .next()
                .fadeIn(0)
                .end()
                .appendTo('#slideshow');
        };


        self.init = function () {
            $("#slideshow > div:gt(0)").hide();
            setInterval(function () {
                if (!isFocus) {
                    self.next();
                }
            }, 5000);

            $(document).on('focus', '#slideshow, a.ctrl', function () {
                isFocus = true;
            });

            $(document).on('blur', '#slideshow, a.ctrl', function () {
                isFocus = false;
            });

            root.find('a.ctrl-prev').on('click', function () {
                self.prev();
            });

            root.find('a.ctrl-next').on('click', function () {
                self.next();
            });
        };

        self.init();

    })($('div.sidebar02'));
});

/*密碼icon*/
$(function () {
    $("input[type='password']").each(function () {
        $(this).after("<div class='input_dwsp_icon'>\
            <i class='far fa-eye' id='" + $(this).attr('id') + "_toggleEye'></i>\
            <i class='fas fa-exclamation-circle' id='" + $(this).attr('id') + "_toggleEC'>\
                <div class=\"CapsLockTip\"><p class=\"CapsLockTip--text\"></p></div>\
            </i>\
        </div>");
        this.classList.add("input_dwsp");
        const dwsp = $(this);
        const dwspEC = $("#" + $(this).attr('id') + "_toggleEC");
        const domdwspEC = document.getElementById(this.id + "_toggleEC");
        const clTip = domdwspEC.getElementsByClassName('CapsLockTip')[0];
        /*密碼顯示開關*/
        $("#" + $(this).attr('id') + "_toggleEye").on('mousedown touchstart', function () {
            dwsp.attr('type', 'text');
            this.classList.add('fa-eye-slash');
        });
        $("#" + $(this).attr('id') + "_toggleEye").on('mouseup mouseleave touchend', function () {
            dwsp.attr('type', 'password');
            this.classList.remove('fa-eye-slash');
        });
        /*密碼大小寫判斷*/
        /*需要新增keyin判斷大寫，並顯示提示字*/
        $(this).on('keypress', function (e) {
            var ev = e ? e : window.event; //event check
            if (!ev) {
                alert("event issue");
            }

            // get key pressed
            var which = -1;
            if (ev.which) {
                which = ev.which;
            } else if (ev.keyCode) {
                which = ev.keyCode;
            }

            // get shift status
            var shift_status = false;
            if (ev.shiftKey) {
                shift_status = ev.shiftKey;
            } else if (ev.modifiers) {
                shift_status = !!(ev.modifiers & 4);
            }

            //啟用了大寫鎖定鍵(Caps Lock)
            if (((which >= 65 && which <= 90) && !shift_status) || ((which >= 97 && which <= 122) && shift_status)) {
                domdwspEC.classList.add('active');
                clTip.classList.add('active');
                clTip.getElementsByClassName('CapsLockTip--text')[0].innerText = "您啟用了大寫鎖定鍵(Caps Lock)";
            }
            else {
                domdwspEC.classList.remove('active');
                clTip.classList.remove('active');
                clTip.getElementsByClassName('CapsLockTip--text')[0].innerText = "";
            }
        });

        /*點擊icon隱藏或顯示提示*/
        dwspEC.on('mousedown', function () {
            if (clTip.classList.contains("active"))
            {
                clTip.classList.remove('active');
                clTip.getElementsByClassName('CapsLockTip--text')[0].innerText = "";
            }
            else
            {
                clTip.classList.add('active');
                clTip.getElementsByClassName('CapsLockTip--text')[0].innerText = "您啟用了大寫鎖定鍵(Caps Lock)";
            }
        });

        /*失焦或點選都關閉Caps Lock提示字*/
        $(this).on('blur mousedown', function (e) {
            clTip.classList.remove('active');
            clTip.getElementsByClassName('CapsLockTip--text')[0].innerText = "";
        });
    });
});
/*GoTop*/
$(function () {
    $("#dvGoTop").on('mousedown touchstart', function () {
        var $body = (window.opera) ? (document.compatMode == "CSS1Compat" ? $('html') : $('body')) : $('html,body');
        $body.animate({ scrollTop: 0 }, 500);
        return false;
    });
    $(window).scroll(function () {
        if ($(this).scrollTop() > 10) {
            $('#dvGoTop').addClass('dvBottom');
            $('#dvSmartAgent').addClass('dvBottom');
        } else {
            $('#dvGoTop').removeClass('dvBottom');
            $('#dvSmartAgent').removeClass('dvBottom');
        }
    });
});

/* 在指定的目標(如: div)上, 加上一層半透明 mask 並顯示 loading 圖示 */
var loadingMaskObj;
var spinnerImgObj;
var defaultSpinnerImgUrl = "/images/progress-bar-gif-13.gif";

$(document).ready(function () {
    spinnerImgObj = createSpinnerImg(defaultSpinnerImgUrl);
});

function loadingMask(targetSelector, imgUrl) {
    var parent = $(targetSelector);
    if (parent.length == 0) {
        parent = $("body");
    }

    var id = "loadingMask_main";
    var cssClass = "loadingMask";
    if (imgUrl == undefined) {
        imgUrl = defaultSpinnerImgUrl;
    }

    parent.find("div#" + id).remove();

    // 動態計算當前頁面 scroll 位置, 以便將 loading 圖示顯示在畫面中
    var $body = (window.opera) ? (document.compatMode == "CSS1Compat" ? $('html') : $('body')) : $('html,body');
    var impTop = $body.scrollTop() + 150;

    var mask = $("<div>");
    mask.attr("id", id)
        .addClass(cssClass)
        .css({
            position: 'absolute',
            top: 0,
            left: 0,
            width: '100%',
            height: parent.height(),
            'background-color': 'rgba(255,255,255,0.3)',
            'z-index': 9999
        });
    
    var innerDiv = $("<div>")
        .css({
            width: '50px',
            height: '50px',
            position: 'absolute',
            left: 0,
            right: 0,
            top: impTop + 'px',
            'margin-left': 'auto',
            'margin-right': 'auto',
            'background-color': 'rgba(255,255,255,0.3)',
            'border-radius': '25px',
            padding: 0,
            overflow: 'clip'
        })
        .appendTo(mask);

    if(spinnerImgObj == undefined || spinnerImgObj.attr('src') != imgUrl) {
        spinnerImgObj = createSpinnerImg(imgUrl);
    }
    spinnerImgObj.appendTo(innerDiv);

    mask.appendTo(parent);

    loadingMaskObj = mask;
}

function loadingMask2(targetSelector, content, imgUrl) {
    var parent = $(targetSelector);
    if (parent.length == 0) {
        parent = $("body");
    }

    var id = "loadingMask_main";
    var cssClass = "loadingMask";
    if (imgUrl == undefined) {
        imgUrl = defaultSpinnerImgUrl;
    }
    if (content == undefined) {
        content = '';
    }
    parent.find("div#" + id).remove();

    // 動態計算當前頁面 scroll 位置, 以便將 loading 圖示顯示在畫面中
    var $body = (window.opera) ? (document.compatMode == "CSS1Compat" ? $('html') : $('body')) : $('html,body');
    var impTop = $body.scrollTop() + 150;

    var mask = $("<div>");
    mask.attr("id", id)
        .addClass(cssClass)
        .css({
            position: 'fixed',
            top: 0,
            left: 0,
            width: '100%',
            height: parent.height(),
            'background-color': 'rgba(255,255,255,0.3)',
            'z-index': 9999
        });

    var innerDiv = $("<div>")
        .attr("id", "loadingMask_div")
        .css({
            width: '50px',
            height: '50px',
            position: 'absolute',
            left: 0,
            right: 0,
            top: getAdTop(50) + 'px',
            'margin-left': 'auto',
            'margin-right': 'auto',
            'background-color': 'rgba(255,255,255,0.3)',
            'border-radius': '25px',
            padding: 0,
            overflow: 'clip'
        })
        .appendTo(mask);

    if (spinnerImgObj == undefined || spinnerImgObj.attr('src') != imgUrl) {
        spinnerImgObj = createSpinnerImg(imgUrl);
    }
    
    spinnerImgObj.appendTo(innerDiv);

    //if (content != '') {
    //    $("<span>")
    //        .attr('alt', content)
    //        .text(content)
    //        .css({
    //            width: '110px',
    //            height: '50px',
    //            'line-height': '80px',
    //            'text-align': 'center',
    //            position: 'absolute',
    //            left: 0,
    //            right: 0,
    //            top: 0,
    //            'margin-left': 'auto',
    //            'margin-right': 'auto'
    //        }).appendTo(innerDiv);
    //}

    mask.appendTo(parent);

    loadingMaskObj = mask;

    $(window).on("scroll resize", function () {
        //控制廣告移動
        $('#loadingMask_div').stop().animate({
            top: getAdTop(50)
        }, 800);
    }).scroll();//啟動scroll
}

function getAdTop(adHeight) {
    return ($(window).height() > adHeight)
        ? ($(window).height() - adHeight) * 0.5
        : 1;
}

function createSpinnerImg(imgUrl) {
    return $("<img>")
            .addClass("spinner")
            .attr('src', imgUrl)
            .attr('alt', 'loading')
            .css({
                width: '50px',
                height: '50px',
                'line-height': '50px',
                'text-align': 'center',
                position: 'absolute',
                left: 0,
                right: 0,
                top: 0,
                'margin-left': 'auto',
                'margin-right': 'auto'
            });
}

function hideLoadingMask() {
    if (loadingMaskObj != undefined) {
        loadingMaskObj.hide();
    }
}

function showLoadingMask() {
    if (loadingMaskObj != undefined) {
        loadingMaskObj.show();
    }
}

function removeLoadingMask() {
    if (loadingMaskObj != undefined) {
        loadingMaskObj.hide();
        loadingMaskObj.remove();
        loadingMaskObj = undefined;
    }
}
