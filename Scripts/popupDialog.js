//====================
//跳窗增強版
//====================
var PopupDialog = {
    //容器
    dialogContainer: $("div#commonDialog"),
    //dialogBody
    dialogBody: $("div#commonDialog").find(".modal-body"),
    //dialogFooter
    dialogFooter: $("div#commonDialog").find(".modal-footer"),
    //關閉視窗後執行的方法
    closeHandler: null,
    //
    errorHandler: null,
    //回傳值
    returnVal: null,
    //
    isProcCloseHandler: false,
    //關窗
    close: function closeDialog() {
        PopupDialog.dialogContainer.modal('hide');
    },
    //跳窗
    show: function (contentUrl, param, title, arrBtn, width, height, closeHandler, errorHandler) {
        //var container = 
        PopupDialog.dialogContainer = $("div#commonDialog");
        var container = $("div#commonDialog");
        //debugger;
        PopupDialog.closeHandler = closeHandler;
        PopupDialog.errorHandler = errorHandler;

        //開窗前清空回傳值
        PopupDialog.returnVal = "";

        if ("commonDialog" !== container.attr("id")) {
            blockAlert("找不到 commonDialog !");
            return;
        }
        if (title) {
            container.find(".modal-title").html(title);
        }

        /*取消關閉按鈕, 先抓出來 clone, 清除 footer 後再加回去, 以確保多次 popupDialog 時, arrBtn按鈕不會重覆*/
        var btnClose = PopupDialog.dialogFooter.find(".btn-close").clone();
        btnClose.on("click", function () {
            container.modal('hide');
        });

        //定義關閉視窗事件
        PopupDialog.isProcCloseHandler = false;
        container.on('hidden.bs.modal', function () {
            //避免關閉事件多次觸發
            if (PopupDialog.isProcCloseHandler) return;
            PopupDialog.isProcCloseHandler = true;
            //執行自訂事件
            if (typeof PopupDialog.closeHandler === "function") {
                PopupDialog.closeHandler(PopupDialog.returnVal);
            }
        })

        PopupDialog.dialogFooter.html("");
        PopupDialog.dialogFooter.append(btnClose);
        /*處理 btnArr 逐一建立並加到 footer 中*/
        if (Array.isArray(arrBtn)) {
            for (var i = 0; i < arrBtn.length; i++) {
                var btn = arrBtn[i];
                if (btn.id && btn.name && btn.onclick) {
                    var a = document.createElement("a");
                    $(a).addClass("tablebtn").addClass("link-pointer");
                    $(a).attr("id", btn.id);
                    $(a).html(btn.name);
                    $(a).on("click", function () {
                        var onclickHandler = window[btn.onclick];
                        if (typeof onclickHandler === "function") {
                            onclickHandler();
                        }
                        else {
                            blockAlert("button '" + btn.name + "' 的 onclick handler '" + btn.onclick + "' 不是正確的 function.");
                            return;
                        }
                    });

                    footer.append(a);
                }
            }
        }

        if (contentUrl) {
            ajaxLoadMore(contentUrl, param, function (data) {
                //debugger;

                //回傳資料為空時, 呼叫 errorHandler
                //if (isEmpty(data)) {
                if (!data) {
                    if (typeof PopupDialog.errorHandler === "function") {
                        PopupDialog.errorHandler();
                        return;
                    }
                }
                //放入資料
                PopupDialog.dialogBody.html(data);

                //高度設定, 避免超過畫面
                if (!$.isNumeric(height)) {
                    height = PopupDialog.dialogContainer.height();
                }
                var windowHeight = Math.floor($(window).height() * 0.7)
                if (height >= windowHeight) {
                    height = windowHeight;
                };
                PopupDialog.dialogBody.css("height", height + "px");

                //寬度設定, 避免超過畫面
                if (!$.isNumeric(width)) {
                    width = PopupDialog.dialogContainer.width();
                }
                //寬度設定, 避免超過畫面
                if ($.isNumeric(width) && width == 600) {
                    container.find(".modal-dialog").css({ width: "78%", margin: "2% auto" });
                }
                else if ($.isNumeric(width) && width == 800) {
                    container.find(".modal-dialog").css({ width: "78%", margin: "2% auto" });
                }
                else {
                    container.find(".modal-dialog").css("width", width + "px");
                }
                var windowWidth = Math.floor($(window).width() * 0.8)
                if ($.isNumeric(width) && $.isNumeric(windowWidth) && width >= windowWidth) {
                    width = windowWidth;
                };

                container.modal('show');
                /*動態載入的 DateTimePicker 要執行 initDatePicker()*/
                container.find("div.date").each(function () {
                    initDatePicker(this);
                });
            });
        }
        else {
            PopupDialog.dialogBody.html("沒有指定 contentUrl");
            container.modal('show');
        }
    }
}