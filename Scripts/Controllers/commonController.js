var Class = function () {
    var klass = function () {
        this.init.apply(this, arguments);
    };

    klass.prototype.init = function () { };

    klass.fn = klass.prototype;

    klass.extend = function (obj) {
        var extended = obj.extended;
        for (var i in obj) {
            klass[i] = obj[i];
        };
        if (extended) extended(klass);
    };

    klass.include = function (obj) {
        var included = obj.included;
        for (var i in obj) {
            klass.fn[i] = obj[i];
        };
        klass.self = obj;
        if (included) included(klass);
    };
    return klass;
};

var CommonController = function (callback) {
    //Class 的屬性：
    callback.include({
        /* Create KendoGrid *  config : kendo grid property */
        createKendoGrid: function (config) {

            function insertbtn(arg) {
                var selected = $.map(this.select(), function (item) {
                    return $(item).text();
                });
                $.ajax({
                    type: "post",
                    url: _LocalhostURL + "/Ajax/InsertUseButton",
                    dataType: "json",
                    data: JSON.stringify({
                        'btid': "",
                        'btname': selected,
                        'url': location.pathname
                    }),
                    contentType: 'application/json; charset=utf-8',
                    async: false,
                    success: function (result) {
                    }
                });
            }

            if (!config) {
                config = {};
            };
            if (!config.obj)
                return;
            var kConfig = config;
            if (typeof (kConfig.scrollable) == "undefined") {
                kConfig.scrollable = true;
            }
            if (typeof (kConfig.sortable) == "undefined") {
                kConfig.sortable = true;
            }
            if (typeof (kConfig.resizable) == "undefined") {
                kConfig.resizable = true;
            }
            if (typeof (kConfig.toolbar) == "undefined") {
                kConfig.toolbar = "";
            }
            if (typeof (kConfig.dataBound) == "undefined") {
                kConfig.dataBound = "";
            }
            if (typeof (kConfig.detailInit) == "undefined") {
                kConfig.detailInit = "";
            }
            if (typeof (kConfig.editable) == "undefined") {
                kConfig.editable = false;
            }
            if (typeof (kConfig.autoBind) == "undefined") {
                kConfig.autoBind = true;
            }
            if (typeof (kConfig.remove) == "undefined") {
                kConfig.remove = null;
            }
            if (typeof (kConfig.save) == "undefined") {
                kConfig.save = null;
            }
            if (typeof (kConfig.cancel) == "undefined") {
                kConfig.cancel = null;
            }
            if (typeof (kConfig.edit) == "undefined") {
                kConfig.edit = null;
            }
            if (typeof (kConfig.dataBinding) == "undefined") {
                kConfig.dataBinding = null;
            }
            var grid = kConfig.obj.kendoGrid({
                scrollable: kConfig.scrollable,
                sortable: kConfig.sortable,
                resizable: kConfig.resizable,
                selectable: "row",
                dataSource: kConfig.dataSource,
                columns: kConfig.columns,
                toolbar: kConfig.toolbar,
                dataBound: kConfig.dataBound,
                detailInit: kConfig.detailInit,
                editable: kConfig.editable,
                autoBind: kConfig.autoBind,
                change: kConfig.autoBind,
                pageable: (kConfig.pageable ? (kConfig.pageableIsCN ? {
                    messages: {
                        display: "{0} - {1} 筆 共： {2}筆",
                        empty: kConfig.emptyMessage,
                        page: "頁",
                        of: "of {0}",
                        itemsPerPage: "items per page",
                        first: "第一頁",
                        previous: "上一頁",
                        next: "下一頁",
                        last: "最後一頁",
                        refresh: "Refresh"
                    }
                } : true) : false),
                remove: kConfig.remove,
                save: kConfig.save,
                cancel: kConfig.cancel,
                edit: kConfig.edit,
                dataBinding: kConfig.dataBinding
            });
        },
        //Reload KendoGrid
        reloadKendoGrid: function (obj, isInitialize) {
            var grid = obj.data("kendoGrid");
            //要求資料來源重新讀取(並指定切至第一頁)
            if (isInitialize) {
                grid.dataSource.page(1);
            }
            else {
                grid.dataSource.read();
            }
            //grid.refresh();
        },
        /* Greate Datepicker
        *  divId : textBox ID
        *  isIcon : 是否要顯示icon
        *  notFuture : 不可以點選明天以後的日期
        */
        createDatepicker: function (divId, isIcon, notFuture) {
            if (isIcon) {
                if (notFuture) {
                    $('#' + divId).datepicker({
                        showOn: "button",
                        buttonImage: _LocalhostURL + "/Content/themes/base/images/calendar_black.png",
                        buttonImageOnly: true,
                        dateFormat: 'yy-mm-dd',
                        changeMonth: true,
                        changeYear: true,
                        yearRange: "-100:+3",
                        maxDate: "0",
                        showOn: "both"
                    });
                }
                else {
                    $('#' + divId).datepicker({
                        showOn: "button",
                        buttonImage: _LocalhostURL + "/Content/themes/base/images/calendar_black.png",
                        buttonImageOnly: true,
                        dateFormat: 'yy-mm-dd',
                        changeMonth: true,
                        changeYear: true,
                        yearRange: "-100:+3",
                        showOn: "both"
                    });
                }
            } else {
                $('#' + divId).datepicker();
            }
        },

        initDataLimit: function () {
            //English and Number 只允許輸入英文數字
            var engAnumArray = $('[data-limit="engAnum"]');
            if (engAnumArray.length > 0) {
                $.each(engAnumArray, function (e, object) {
                    //清空表示註冊完成
                    $(object).attr('data-limit', '')

                    //不能使用輸入法 只能用在IE Firefox
                    $(object).css('ime-mode', 'disabled');

                    $(object).on("input", function (e) {
                        var self = $(this)[0];
                        self.value = self.value.replace(/[^\w\.\/]/ig, '');
                    });
                });
            }

            //English and Mandarin 只允許輸入中英文
            var engAmanArray = $('[data-limit="engAman"]');
            if (engAmanArray.length > 0) {
                $.each(engAmanArray, function (e, object) {
                    //清空表示註冊完成
                    $(object).attr('data-limit', '')

                    $(object).on("input", function (e) {
                        var self = $(this)[0];
                        self.value = self.value.replace(/0/i, '');
                        self.value = self.value.replace(/1/i, '');
                        self.value = self.value.replace(/2/i, '');
                        self.value = self.value.replace(/3/i, '');
                        self.value = self.value.replace(/4/i, '');
                        self.value = self.value.replace(/5/i, '');
                        self.value = self.value.replace(/6/i, '');
                        self.value = self.value.replace(/7/i, '');
                        self.value = self.value.replace(/8/i, '');
                        self.value = self.value.replace(/9/i, '');
                    });
                });
            }

            //Money Limit 金額的輸入限制
            var moneyArray = $('[data-limit="money"]');
            if (moneyArray.length > 0) {
                $.each(moneyArray, function (e, object) {
                    //加入css 金錢靠右
                    //$(object).css('text-align', 'right');
                    //清空表示註冊完成
                    $(object).attr('data-limit', '')

                    //不能使用輸入法 只能用在IE Firefox
                    $(object).css('ime-mode', 'disabled');

                    $(object).on("keydown", function (e) {
                        var key;
                        //console.warn(e.keyCode);
                        //console.warn(e.which);
                        if (window.event) {
                            key = e.keyCode;
                        } else if (e.which) {
                            key = e.which;
                        } else {
                            return true;
                        }
                        //console.log(key);
                        if ((key >= 48 && key <= 57)
                            || (key >= 96 && key <= 105) //數字鍵盤
                            || 8 == key || 46 == key || 37 == key || 39 == key //8:backspace 46:delete 37:左 39:右 (倒退鍵、刪除鍵、左、右鍵也允許作用)
                            || 110 == key || 9 == key) {
                            return true;
                        } else {
                            return false;
                        }
                    });

                    //keyup事件會導致 Edge 的change事件無效
                    ////防新注音的123456789
                    //$(object).on("keyup", function (e) {
                    //    var self = $(this);
                    //    self.val(/^\d+[.]?\d*/.exec(self.val()));

                    //});

                    //點下TextBox時全選
                    $(object).on("click", function () {
                        var self = $(this);
                        self[0].value = self.val().replace(/[^0-9\.]+/g, "");
                        self.select();
                    });

                    //離開TextBox時轉換
                    $(object).on("focusout", function () {
                        var self = $(this);
                        //金錢格式轉數字
                        var number = self.val().replace(/[^0-9\.]+/g, "");
                        var money = ((Number)(number)).toFixed(0);
                        self[0].value = money.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,")
                    });
                });
            }

            //數字格式
            var numberArray = $('[data-limit="number"]');
            if (numberArray.length > 0) {
                $.each(numberArray, function (e, object) {
                    ////加入css 數字靠右
                    //$(object).css('text-align', 'right');
                    //不能使用輸入法 只能用在IE Firefox
                    $(object).css('ime-mode', 'disabled');

                    //清空表示註冊完成
                    $(object).attr('data-limit', '');

                    $(object).on("keydown", function (e) {
                        var key;
                        //console.warn(e.keyCode);
                        //console.warn(e.which);
                        if (window.event) {
                            key = e.keyCode;
                        } else if (e.which) {
                            key = e.which;
                        } else {
                            return true;
                        }
                        //console.log(key);
                        if ((key >= 48 && key <= 57)
                            || (key >= 96 && key <= 105) //數字鍵盤
                            || 8 == key || 46 == key || 37 == key || 39 == key //8:backspace 46:delete 37:左 39:右 (倒退鍵、刪除鍵、左、右鍵也允許作用)
                            || 110 == key || 9 == key) {
                            return true;
                        } else {
                            return false;
                        }
                    });
                    ////防新注音的123456789
                    //$(object).on("keyup", function (e) {
                    //    var self = $(this);
                    //    self.val(/^\d+[.]?\d*/.exec(self.val()));

                    //});

                    //點下TextBox時全選
                    $(object).on("click", function () {
                        var self = $(this);
                        self.select();
                    });
                });
            }

            // 只允許輸入數字 (防複製貼上)
            $('input[data-control="number"]').on('compositionend change', function () {
                let value = this.value;
                let numericRegex = /^[0-9]*$/;
                if (!numericRegex.test(value)) {
                    this.value = value.replace(/[^0-9]/g, '');
                }
            });

            // 電話或傳真格式 (可輸入區碼或分機 只允許數字及-和# 例:02-123456789#123)
            $('input[data-control="phoneOrFax"]').on('compositionend change', function () {
                let value = this.value;
                let numericRegex = /^[0-9-#]*$/;
                if (!numericRegex.test(value)) {
                    this.value = value.replace(/[^0-9-#]/g, '');
                }
            });
        },

        showBootboxDialog: function (message, btnClass, callback) {
            bootbox.dialog({
                message: message,
                buttons: {
                    danger: {
                        label: "確定",
                        className: btnClass,
                        callback: callback
                    }
                }
            });
        },

        /* Create Dialog
        *  obj : dialog object
        *  title : 標題
        *  width : 寬度
        *  autoOpen : 是否自動開啟
        */
        createDialog: function (obj, title, width, autoOpen) {
            $('body').addClass('stop-scrolling');
            $('html, body').scrollTop(0);
            var cTitle = "";
            if (typeof (title) != "undefined") {
                cTitle = title;
            }
            var cWidth = 500;
            if (typeof (width) != "undefined") {
                cWidth = width;
            }
            var cAutoOpen = false;
            if (typeof (autoOpen) != "undefined") {
                cAutoOpen = autoOpen;
            }

            obj.dialog({
                autoOpen: cAutoOpen,
                draggable: true,
                resizable: false,
                close: function () {
                    $('body').removeClass('stop-scrolling');
                }, title: cTitle, width: cWidth, modal: true
            });
        },

        //修改多筆資料時資料狀態
        //isNew 是否從資料庫讀出來
        //status 將要改成什麼狀態
        //新增：1， 刪除：2 ，修改：3，無修改：4
        getEditStatus: function (isNew, status) {
            var result;
            switch (status) {
                case callback.self.editStatus.Modify:
                    if (isNew) {
                        result = callback.self.editStatus.Add;
                    } else {
                        result = callback.self.editStatus.Modify;
                    }
                    break;
                default:
                    result = status
                    break;
            }
            return result;
        },

        //編輯狀態
        editStatus: {
            Add: 1,
            Delete: 2,
            Modify: 3,
            NoChange: 4
        },

        //回傳時顯示的訊息
        resultMessage: function (resultData) {
            if (!resultData.IsSuccess) {
                //錯誤訊息內容
                if (resultData.IsDebug) {
                    console.log(resultData.resultDataExceptionMessage);
                    console.log(resultData.ExceptionStackTrace);
                }
            }
            //Show訊息
            alert(resultData.Message);
        },

        //多筆編輯設定資料為新增
        setVMIsAdd: function (newObj, oldVM) {
            //設定操作用的Index
            newObj.Index = oldVM.length;
            /// <summary從資料庫取得為false</summary>
            newObj.IsNew = true;
            /// <summary>資料操作狀態</summary>
            var editStatus = callback.self.editStatus.Add;
            newObj.Status = callback.self.getEditStatus(true, editStatus);
            //新增進VM
            oldVM.push(newObj);
        },

        //多筆編輯設定資料為編輯
        setVMIsModify: function (newobj, oldObj, oldVM) {
            var data = oldVM[oldObj.Index];

            //更新Model資料
            var keys = Object.keys(newobj);
            $.each(keys, function (index, item) {
                data[item] = newobj[item];
            });

            /// <summary>資料操作狀態</summary>
            var editStatus = callback.self.editStatus.Modify;
            data.Status = callback.self.getEditStatus(data.IsNew, editStatus);
        },

        //多筆編輯設定資料為刪除
        setVMIsDelete: function (obj, oldVM) {
            var data = oldVM[obj.Index];
            var editStatus = callback.self.editStatus.Delete;
            data.Status = callback.self.getEditStatus(data.IsNew, editStatus);
        },

        //動態取得頁面
        dynamicallyGetThePage: function (controller, action, traget, parament) {
            //清空
            traget.empty();
            if (parament == null) {
                $.ajax({
                    type: "post",
                    url: _LocalhostURL + "/" + controller + "/" + action,
                    dataType: "json",
                    contentType: 'application/json; charset=utf-8',
                    success: function (response) {
                        //塞入
                        traget.html(response);
                    }
                });
            } else {
                $.ajax({
                    type: "post",
                    url: _LocalhostURL + "/" + controller + "/" + action,
                    dataType: "json",
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify({
                        'parament': parament
                    }),
                    success: function (response) {
                        //塞入
                        traget.html(response);
                    }
                });
            }
        },

        //Form UI 操作動作
        formOperationType: {
            //取得資料
            GetData: 0,
            //塞資料
            SetData: 1,
            //清空
            Clean: 2
        },

        //Form UI 操作 (參數：target=$(div),type=FormOperationType)
        formOperation: function (target, formOperationType, data) {
            data = data || null;

            target = target.find('[data-field]');

            //當取得物件時使用
            var newObj = {
            };

            $.each(target, function (index, item) {
                var tagName = $(item).prop('tagName');
                switch (tagName) {
                    case "INPUT":
                        var type = $(item).prop('type');
                        switch (type) {
                            case "text":
                                callback.self.textUI($(item), formOperationType, data, newObj);
                                break;
                            case "checkbox":
                                callback.self.checkboxUI($(item), formOperationType, data, newObj);
                                break;
                            case "radio":
                                callback.self.radioUI($(item), formOperationType, data, newObj);
                                break;
                            case "hidden":
                                callback.self.hiddenUI($(item), formOperationType, data, newObj);
                                break;
                        }
                        break;
                    case "LABEL":
                        callback.self.labelUI($(item), formOperationType, data, newObj);
                        break;
                    case "TEXTAREA":
                        callback.self.textAreaUI($(item), formOperationType, data, newObj);
                        break;
                    case "SELECT":
                        callback.self.selectUI($(item), formOperationType, data, newObj);
                        break;
                }
            });

            return newObj;
        },

        //操作UI 參數：(control=控制項,formOperationType=操作動作,data=填入的資料,newObj=回傳的資料)
        textUI: function (control, formOperationType, data, newObj) {
            var field = control.attr('data-field');
            var type = control.attr('data-type');

            switch (formOperationType) {
                case callback.self.formOperationType.GetData:

                    if (type == "money") {
                        newObj[field] = control.val() === "" ? "" : callback.self.currencyToNumber(control.val());
                    }
                    else if (type == "number") {
                        newObj[field] = control.val() === "" ? "" : Number(control.val());
                    }
                    else if (type == "date") {
                        newObj[field] = control.val() === "" ? "" : callback.self.twDateToBCDate(control.val());
                    }
                    else {
                        newObj[field] = control.val().trim();
                    }
                    break;
                case callback.self.formOperationType.SetData:
                    if (type == "money") {
                        control.val(data[field] == null ? "" : callback.self.numberToCurrency(data[field]));
                    }
                    else if (type == "date") {
                        control.val(data[field] == null ? "" : callback.self.bcDateToTWDate(data[field]));
                    }
                    else {
                        control.val(data[field]);
                    }
                    break;
                case callback.self.formOperationType.Clean:
                    control.val("");
                    break;
            }
        },

        //操作UI 參數：(control=控制項,formOperationType=操作動作,data=填入的資料,newObj=回傳的資料)
        checkboxUI: function (control, formOperationType, data, newObj) {
            var field = control.attr('data-field');
            switch (formOperationType) {
                case callback.self.formOperationType.GetData:
                    var checked = control.prop('checked');
                    if (checked) {
                        if (newObj[field] == null) {
                            newObj[field] = [];
                            newObj[field].push(control.val().trim());
                        } else {
                            newObj[field].push(control.val().trim());
                        }
                    }
                    break;
                case callback.self.formOperationType.SetData:
                    if ($.inArray(control.val().trim(), data[field]) > -1) {
                        control.prop('checked', true);
                    } else {
                        control.prop('checked', false);
                    }
                    break;
                case callback.self.formOperationType.Clean:
                    control.prop('checked', false);
                    break;
            }
        },

        //操作UI 參數：(control=控制項,formOperationType=操作動作,data=填入的資料,newObj=回傳的資料)
        radioUI: function (control, formOperationType, data, newObj) {
            var field = control.attr('data-field');
            switch (formOperationType) {
                case callback.self.formOperationType.GetData:
                    var checked = control.prop('checked');
                    if (checked) {
                        newObj[field] = control.val().trim();
                    }
                    break;
                case callback.self.formOperationType.SetData:
                    var val = control.val().trim();
                    if (val == data[field]) {
                        control.prop('checked', true);
                    }
                    break;
                case callback.self.formOperationType.Clean:
                    control.prop('checked', false);
                    break;
            }
        },

        //操作UI 參數：(control=控制項,formOperationType=操作動作,data=填入的資料,newObj=回傳的資料)
        labelUI: function (control, formOperationType, data, newObj) {
            var field = control.attr('data-field');
            var type = control.attr('data-type');
            switch (formOperationType) {
                case callback.self.formOperationType.GetData:
                    if (type == "money") {
                        newObj[field] = callback.self.currencyToNumber(control.text());
                    }
                    else if (type == "number") {
                        newObj[field] = Number(control.text());
                    } else if (type == "date") {
                        newObj[field] = callback.self.twDateToBCDate(control.text());
                    }
                    else {
                        newObj[field] = control.text().trim();
                    }
                    break;
                case callback.self.formOperationType.SetData:
                    if (type == "money") {
                        control.text(data[field] == null ? "" : callback.self.numberToCurrency(data[field]));
                    }
                    else if (type == "date") {
                        control.text(data[field] == null ? "" : callback.self.bcDateToTWDate(data[field]));
                    }
                    else {
                        control.text(data[field] == null ? "" : data[field]);
                    }
                    break;
                case callback.self.formOperationType.Clean:
                    control.text("");
                    break;
            }
        },

        //操作UI 參數：(control=控制項,formOperationType=操作動作,data=填入的資料,newObj=回傳的資料)
        textAreaUI: function (control, formOperationType, data, newObj) {
            var field = control.attr('data-field');
            switch (formOperationType) {
                case callback.self.formOperationType.GetData:
                    newObj[field] = control.val().trim();
                    break;
                case callback.self.formOperationType.SetData:
                    control.val(data[field]);
                    break;
                case callback.self.formOperationType.Clean:
                    control.val("");
                    break;
            }
        },

        //操作UI 參數：(control=控制項,formOperationType=操作動作,data=填入的資料,newObj=回傳的資料)
        selectUI: function (control, formOperationType, data, newObj) {
            var field = control.attr('data-field');
            var fieldText = control.attr('data-fieldText');
            var type = control.attr('data-type');
            switch (formOperationType) {
                case callback.self.formOperationType.GetData:
                    if (type == "date") {
                        newObj[field] = Number(control.val()) + 1911;
                    } else {
                        newObj[field] = control.val();
                        if (fieldText != null) {
                            newObj[fieldText] = control.find(':selected').text();
                        }
                    }
                    break;
                case callback.self.formOperationType.SetData:
                    if (type == "date") {
                        control.val(Number(data[field]) - 1911);
                    } else {
                        control.val(data[field]);
                        control.attr("data-value", data[field]);
                        if (control.attr("data-event") == "change") {
                            control.trigger("change");
                        }
                    }
                    break;
                case callback.self.formOperationType.Clean:
                    control.val("0");
                    break;
            }
        },


        //操作UI 參數：(control=控制項,formOperationType=操作動作,data=填入的資料,newObj=回傳的資料)
        hiddenUI: function (control, formOperationType, data, newObj) {
            var field = control.attr('data-field');
            switch (formOperationType) {
                case callback.self.formOperationType.GetData:
                    newObj[field] = control.val().trim();
                    break;
                case callback.self.formOperationType.SetData:
                    control.val(data[field]);
                    break;
                case callback.self.formOperationType.Clean:
                    control.val("");
                    break;
            }
        },

        //將三位一撇轉為數值(小數點第二位)
        currencyToNumber: function (currency, pos) {
            if (pos == null) { pos = 2; }

            if (currency == undefined) { currency = ""; }
            //金錢格式轉數字
            currency = currency.replace(/[^0-9\.]+/g, "");
            //取到小數點第二位
            currency = ((Number)(currency)).toFixed(pos);
            return Number(currency);
        },

        //將數值轉為三位一撇(預設小數點第二位，pos為小數點位數) //預設為小數點第0位 2015 11 19
        numberToCurrency: function (number, pos) {
            if (typeof (number) == "string") { number = Number(number); }

            var point = 0;

            if (typeof pos != "undefined") { point = pos; }

            var money = number.toFixed(point);

            var moneyStr = String(money);
            //整數
            var interger = money.split(".")[0].replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
            //小數
            var decimal = money.split(".")[1] != null ? "." + money.split(".")[1] : "";

            return interger + decimal;
        },
        //檔案上傳 Grid 建制
        createUploadGrid: function (target, data, callBackMethod, dataBoundExtension) {
            function createGrid() {
                var dataSource = {
                    type: "json",
                    pageSize: 3,
                    schema: {
                        data: function (d) {
                            return data;
                        },
                        total: function (d) {
                            return data.length;
                        }
                    }
                };
                target.empty();
                //定義欄位
                var columns = [
                    {
                        headerAttributes: {
                            style: "display:none;"
                        },
                        attributes: {
                            style: "display:none;"
                        },
                        width: "25%"
                    },
                    {
                        headerAttributes: {
                            style: "white-space:normal;text-align: center;"
                        },
                        field: "DisplayFileName",
                        title: "檔案名稱",
                        width: "100%",
                        attributes: {
                            style: "text-align: center;"
                        }
                    },
                    {
                        headerAttributes: {
                            style: "white-space:normal;text-align: center;"
                        },
                        title: "刪除",
                        width: "75%",
                        attributes: {
                            style: "text-align: center;"
                        },
                        template: function (e) {
                            var btn = '';
                            if (e.Download == true) {
                                btn = '<a href="' + _LocalhostURL + "/PApplyCourse/DownLoad?sV_Filename=" + e.FileName + "&" + "uS_Filename=" + e.DisplayFileName + '" class="btn btn-danger" style="color:white;" target="_blank">下載</a>';
                            }
                            btn += '<input type="button" class="btn btn-danger" data-btn="deleteUpload" value="刪除" />';
                            return btn;
                        }
                    }
                ];

                callback.self.createKendoGrid({
                    obj: target,
                    pageable: true,
                    pageableIsCN: true,
                    sortable: false,
                    dataSource: dataSource,
                    columns: columns,
                    emptyMessage: "無資料顯示",
                    scrollable: false,
                    resizable: false,
                    dataBound: function () { uploadDataBound(dataBoundExtension); }
                });
                if (target.length > 0) {
                    var grid = target.data("kendoGrid");
                    var wrapper = $('<div class="k-pager-wrap k-grid-pager pagerTop"/>').insertBefore(grid.element.children("table"));
                    grid.pagerTop = new kendo.ui.Pager(wrapper, $.extend({}, grid.options.pageable, { dataSource: grid.dataSource }));
                    grid.element.height("").find(".pagerTop").css("border-width", "0 0 1px 0");
                }
            }
            //databound 處理callbackfunction
            function uploadDataBound() {

                //外部傳入的擴充功能
                if (dataBoundExtension != null) {
                    dataBoundExtension();
                }

                target.find('[data-btn="deleteUpload"]').on('click', function () {
                    var self = $(this);

                    var editRow = target.data("kendoGrid").dataItem(self.parent().parent());

                    callBackMethod(editRow);
                });
            }

            //建立Grid
            createGrid();
        },
        //建立 Kendo Upload
        createUpload: function (targetObject, callbackFunction, isAlert) {
            targetObject.kendoUpload({
                localization: {
                    select: "選擇檔案"
                },
                showFileList: false,
                multiple: true,
                select: function (e) {
                },
                async: {
                    saveUrl: _LocalhostURL + "/Shared/UploadFile",
                    autoUpload: true
                },
                success: function (e) {
                    //檔名
                    if (e.response.Message === "Y") {
                        var filesName = e.files[0].name

                        var attachment = {
                            DisplayFileName: filesName,
                            FileName: e.response.FileName,
                            success: true
                        };
                    } else {
                        var attachment = {
                            DisplayFileName: "",
                            FileName: "",
                            success: false
                        };
                    }
                    if (callbackFunction != undefined) {
                        callbackFunction(attachment);
                    }

                    if (isAlert != false) {
                        alert(e.response.Info);
                    }
                },
                //complete: function (e) {
                //    alert('完成');

                //}
            });
        },
        //回頁面最上層
        goTop: function () {
            $('html,body').animate({
                scrollTop: 0
            }, 200);
        },
        //Html轉報表(target $(div))
        convertToReport: function (target) {
            //鎖住
            target.find(':input').prop('disabled', true);
            //控制項切換
            //textbox切換
            var textBox = $('#divReport').find('[type="text"]');
            $.each(textBox, function (index, item) {
                var tempValue = $(item).val();
                $(item).after('<label><u>' + tempValue + '</u></label>');
                $(item).remove();
            });
            //浮水印
        },
        //產生驗證碼
        passwordGenerator: (function () {
            var generateRandomNum = function (max) {
                var crypto = window.crypto || window.msCrypto;
                if (!crypto) {
                    throw new Error('Unsupported browser.');
                }
                var array = new Uint8Array(1);
                crypto.getRandomValues(array);
                var range = max + 1;
                var max_range = 256;
                if (array[0] >= Math.floor(max_range / range) * range)
                    return generateRandomNum(max);
                return (array[0] % range);
            };
            /*
            options.passwordLength: 6,  //驗證碼長度
            options.includeUppercaseChars: false,  //是否有大寫英文字母
            options.includeLowercaseChars: false,  //是否有小寫英文字母
            options.includeNumbers: true,    //是否有數字
            options.includeSpecialChars: false,   //是否有特殊符號
            */
            var generatePassword = function (options) {
                var uppercase = "ABCDEFGHJKMNPQRSTUVWXYZ";
                var lowercase = "abcdefghjkmnpqrstuvwxyz";
                var numbers = "23456789";
                var special = "!#$%&*?";
                var candidates = '';
                if (options.includeUppercaseChars) {
                    candidates += uppercase;
                }
                if (options.includeLowercaseChars) {
                    candidates += lowercase;
                }
                if (options.includeNumbers) {
                    candidates += numbers;
                }
                if (options.includeSpecialChars) {
                    candidates += special;
                }
                var password = `${Date.now()}${String(Math.floor(Math.random() * 1000)).padStart(3, '0')}`;
                for (var i = 0; i < options.passwordLength; i++) {
                    var randomNum = generateRandomNum(candidates.length);
                    password += candidates.substring(randomNum, randomNum + 1);
                }
                return password;
            };
            return {
                generatePassword: generatePassword
            };
        })(),
        //民國年轉西元年
        twDateToBCDate: function (date) {
            if (date != null && date != "") {
                var twDateArr = date.split('/');
                var bcDate = Number(twDateArr[0]) + 1911 + "/" + twDateArr[1] + "/" + twDateArr[2];
                return bcDate;
            } else {
                return date;
            }
        },
        //西元年轉民國年
        bcDateToTWDate: function (date) {
            if (date != null && date != "") {
                var bcDateArr = date.split('/');
                var twDate = Number(bcDateArr[0]) - 1911 + "/" + bcDateArr[1] + "/" + bcDateArr[2];
                return twDate;
            } else {
                return date;
            }
        },
        //檢查統一編號是否正確
        isValidGUI: function (taxId) {
            var invalidList = "00000000,11111111";
            if (/^\d{8}$/.test(taxId) == false || invalidList.indexOf(taxId) != -1) {
                return false;
            }

            var validateOperator = [1, 2, 1, 2, 1, 2, 4, 1],
                sum = 0,
                calculate = function (product) { // 個位數 + 十位數
                    var ones = product % 10,
                        tens = (product - ones) / 10;
                    return ones + tens;
                };
            for (var i = 0; i < validateOperator.length; i++) {
                sum += calculate(taxId[i] * validateOperator[i]);
            }

            return sum % 10 == 0 || sum % 5 == 0 || (taxId[6] == "7" && (sum + 1) % 10 == 0) || (taxId[6] == "7" && (sum + 1) % 5 == 0);
        },
        //檢查身分證字號是否正確
        isValidIC: function (ic) {
            var city = new Array(
                1, 10, 19, 28, 37, 46, 55, 64, 39, 73, 82, 2, 11,
                20, 48, 29, 38, 47, 56, 65, 74, 83, 21, 3, 12, 30)
            ic = ic.toUpperCase();
            if (ic.search(/^[A-Z](1|2|8|9)\d{8}$/i) == -1) {
                return false;
            } else {
                ic = ic.split('');//字串分割為陣列(for IE)
                var total = city[ic[0].charCodeAt(0) - 65];
                for (var intTime = 1; intTime <= 8; intTime++) {
                    total += eval(ic[intTime]) * (9 - intTime);
                }
                total += eval(ic[9]);//檢查碼(最後一碼)
                return ((total % 10 == 0));//檢查比對碼
            }
        },
        //檢查居留證字號是否正確
        isValidRC: function (ic) {
            studIdNumber = ic.toUpperCase();

            //驗證填入身分證字號長度及格式
            if (studIdNumber.length != 10) {
                return false;
            }
            //格式，用正則表示式比對第一個字母是否為英文字母
            if ((isNaN(studIdNumber.substr(2, 8)) || (!/^[A-Z]$/.test(studIdNumber.substr(0, 1))) || (!/^[A-Z]$/.test(studIdNumber.substr(1, 1))))
                &&
                (studIdNumber.search(/^[A-Z](1|2|8|9)\d{8}$/i) == -1)) {
                return false;
            }

            if (studIdNumber.search(/^[A-Z](1|2|8|9)\d{8}$/i) == 0) {
                return callback.self.isValidIC(studIdNumber)
            }
            else {

                var idHeader = "ABCDEFGHJKLMNPQRSTUVXYWZIO"; //按照轉換後權數的大小進行排序
                //這邊把身分證字號轉換成準備要對應的
                studIdNumber = (idHeader.indexOf(studIdNumber.substring(0, 1)) + 10) +
                    '' + ((idHeader.indexOf(studIdNumber.substr(1, 1)) + 10) % 10) + '' + studIdNumber.substr(2, 8);
                //開始進行身分證數字的相乘與累加，依照順序乘上1987654321

                s = parseInt(studIdNumber.substr(0, 1)) +
                    parseInt(studIdNumber.substr(1, 1)) * 9 +
                    parseInt(studIdNumber.substr(2, 1)) * 8 +
                    parseInt(studIdNumber.substr(3, 1)) * 7 +
                    parseInt(studIdNumber.substr(4, 1)) * 6 +
                    parseInt(studIdNumber.substr(5, 1)) * 5 +
                    parseInt(studIdNumber.substr(6, 1)) * 4 +
                    parseInt(studIdNumber.substr(7, 1)) * 3 +
                    parseInt(studIdNumber.substr(8, 1)) * 2 +
                    parseInt(studIdNumber.substr(9, 1));

                //檢查號碼 = 10 - 相乘後個位數相加總和之尾數。
                checkNum = parseInt(studIdNumber.substr(10, 1));
                //模數 - 總和/模數(10)之餘數若等於第九碼的檢查碼，則驗證成功
                ///若餘數為0，檢查碼就是0
                if ((s % 10) == 0 || (10 - s % 10) == checkNum) {
                    return true;
                }
                else {
                    return false;
                }
            }
        },
        //檢查手機格式是否正確
        isCellPhone: function (numbers) {
            //驗證填入長度
            if (numbers.length != 10) {
                return false;
            }
            else if (!/^09[0-9]{8}$/.test(numbers)) {
                return false;
            }
            else {
                return true;
            }
        },
        //數字轉國字(到十位數)
        numberToWord: function (number) {
            //個位
            var workd1 = function (numberChar) {
                var result = "";
                switch (numberChar) {
                    case '1': result = "一";
                        break;
                    case '2': result = "二";
                        break;
                    case '3': result = "三";
                        break;
                    case '4': result = "四";
                        break;
                    case '5': result = "五";
                        break;
                    case '6': result = "六";
                        break;
                    case '7': result = "七";
                        break;
                    case '8': result = "八";
                        break;
                    case '9': result = "九";
                        break;
                    default: result = "";
                        break;
                }
                return result;
            }
            //十位
            var workd2 = function (numberChar) {
                var result = "";
                switch (numberChar) {
                    case '1': result = "十";
                        break;
                    case '2': result = "二十";
                        break;
                    case '3': result = "三十";
                        break;
                    case '4': result = "四十";
                        break;
                    case '5': result = "五十";
                        break;
                    case '6': result = "六十";
                        break;
                    case '7': result = "七十";
                        break;
                    case '8': result = "八十";
                        break;
                    case '9': result = "九十";
                        break;
                }
                return result;
            }
            var resultWord = "";
            var arrNumber = String(number).split('');
            if (number > 9) {
                resultWord = workd2(arrNumber[0]) + workd1(arrNumber[1]);
            } else {
                resultWord = workd1(arrNumber[0]);
            }
            return resultWord;
        },
        //浮點數相加
        floatAdd: function (arg1, arg2) {
            var r1, r2, m;
            try { r1 = arg1.toString().split(".")[1].length; } catch (e) { r1 = 0; }
            try { r2 = arg2.toString().split(".")[1].length; } catch (e) { r2 = 0; }
            m = Math.pow(10, Math.max(r1, r2));
            return (callback.self.floatMul(arg1, m) + callback.self.floatMul(arg2, m)) / m;
        },
        //浮點數相減
        floatSubtraction: function (arg1, arg2) {
            var r1, r2, m, n;
            try { r1 = arg1.toString().split(".")[1].length } catch (e) { r1 = 0 }
            try { r2 = arg2.toString().split(".")[1].length } catch (e) { r2 = 0 }
            m = Math.pow(10, Math.max(r1, r2));
            n = (r1 >= r2) ? r1 : r2;
            return ((arg1 * m - arg2 * m) / m).toFixed(n);
        },
        //浮點數相乘
        floatMul: function (arg1, arg2) {
            var m = 0, s1 = arg1.toString(), s2 = arg2.toString();
            try { m += s1.split(".")[1].length; } catch (e) { }
            try { m += s2.split(".")[1].length; } catch (e) { }
            return Number(s1.replace(".", "")) * Number(s2.replace(".", "")) / Math.pow(10, m);
        },
        //浮點數相除
        floatDiv: function (arg1, arg2) {
            var t1 = 0, t2 = 0, r1, r2;
            try { t1 = arg1.toString().split(".")[1].length } catch (e) { }
            try { t2 = arg2.toString().split(".")[1].length } catch (e) { }
            with (Math) {
                r1 = Number(arg1.toString().replace(".", ""))
                r2 = Number(arg2.toString().replace(".", ""))
                return (r1 / r2) * pow(10, t2 - t1);
            }
        },
        // 使用 replace 搭配正規表達式來尋找單一數字的月份和日期
        formatMyDate: function (dateString) {
            // \b(\d{1})\b 匹配獨立的單一數字 (例如 8 或 9) // '0$1' 表示在匹配到的數字前加上 '0'
            return dateString.replace(/\b(\d{1})\b/g, '0$1');
        }
    });
    return callback;
}


