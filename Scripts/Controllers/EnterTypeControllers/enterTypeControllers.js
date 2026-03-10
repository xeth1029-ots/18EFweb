
var EnterTypeController = function (callback) {
    //Class 的屬性：
    callback.include({
        //注入設定
        config: {},

        //初始化
        init: function (config) {
            callback.self.config = config;

            //執行input格式限制
            config.commonController.fn.initDataLimit();

            //金融機構代碼
            $('#selectBank').on('change', function () {
                $('#selectBranch').empty();
                callback.self.getBankList($(this).val());
                callback.self.getBranchList($(this).val());
            });

            //分行代碼
            $('#selectBranch').on('change', function () {
                callback.self.getBranchList($('#selectBank').val(), $(this).val());
            });

            //註冊Datepicker //config.commonController.fn.createDatepicker("textCompanyCreateDate", true, true);

            //設定資料
            if (config.editData != null) {
                callback.self.setViewData(config.editData);
                callback.self.getBankList(config.editData.BankID);
                callback.self.getBranchList(config.editData.BankID, config.editData.BranchID);
                $('input[data-control="number"]').trigger('change');
            }

            //銀行名稱(固定)
            var availableTags = [
                "臺灣銀行",
                "土地銀行",
                "合作金庫商業銀行",
                "第一商業銀行",
                "華南商業銀行",
                "彰化商業銀行",
                "上海商業儲蓄銀行",
                "台北富邦商業銀行",
                "國泰世華商業銀行",
                "高雄銀行",
                "兆豐國際商業銀行",
                "花旗（台灣）商業銀行",
                "澳盛（台灣）商業銀行",
                "王道商業銀行",
                "臺灣中小企業銀行",
                "渣打國際商業銀行",
                "台中商業銀行",
                "京城商業銀行",
                "滙豐（台灣）商業銀行",
                "瑞興商業銀行",
                "華泰商業銀行",
                "臺灣新光商業銀行",
                "陽信商業銀行",
                "板信商業銀行",
                "三信商業銀行",
                "聯邦商業銀行",
                "遠東國際商業銀行",
                "元大商業銀行",
                "永豐商業銀行",
                "玉山商業銀行",
                "凱基商業銀行",
                "星展（台灣）商業銀行",
                "台新國際商業銀行",
                "日盛國際商業銀行",
                "安泰商業銀行",
                "中國信託商業銀行",
                "中國輸出入銀行",
                "全國農業金庫",
                "中華郵政"
            ];
            //金融機構名稱
            $("#textBankName").autocomplete({
                source: availableTags
            });

            //圖片上傳
            $('#submitBtn').on('click', callback.self.uploadPhotoWithBoxes);

            //Assuming you have a button to trigger this function //$('#uploadButton').on('click', callback.self.uploadPhotoWithBoxes);
        },

        BankName: '',
        BranchName: '',

        //-=-=-

        //取得Form資料
        getViewData: function () {
            var viewModel = {};
            //加密ACID
            viewModel.Epi = $('#Detail_epi').val();
            viewModel.Epb = $('#Detail_epb').val();
            viewModel.Ept = $('#Detail_ept').val();
            /// <summary>金融機構代碼</summary>
            //viewModel.BankID = $('#textBankCode').val().trim();
            viewModel.BankID = $('#selectBank').val();
            /// <summary>銀行代碼+分行代碼</summary>
            //viewModel.BranchID = $('#textBranchNumber').val().trim();
            viewModel.BranchID = $('#selectBranch').val();
            /// <summary>金融機構名稱</summary>
            viewModel.BankName = $('#textBankName').val().trim();
            /// <summary>分局名稱/分行名稱</summary>
            viewModel.BranchName = $('#textBranchName').val().trim();
            /// <summary>匯入帳號</summary>
            viewModel.BankAccount = $('#textBankAccount').val().trim();

            return viewModel;
        },

        //設定資料進View
        setViewData: function (data) {
            //加密ACID
            $('#Detail_epi').val(data.Epi);
            $('#Detail_epb').val(data.Epb);
            $('#Detail_ept').val(data.Ept);
            /// <summary>金融機構代碼</summary>
            //$('#textBankCode').val(data.BankID);
            $('#selectBank').val(data.BankID);
            /// <summary>銀行代碼+分行代碼</summary>
            //$('#textBranchNumber').val(data.BranchID);
            $('#selectBranch').val(data.BranchID);
            /// <summary>金融機構名稱</summary>
            $('#textBankName').val(data.BankName);
            /// <summary>分局名稱/分行名稱</summary>
            $('#textBranchName').val(data.BranchName);
            /// <summary>匯入帳號</summary>
            $('#textBankAccount').val(data.BankAccount);
        },

        //取得銀行list
        getBankList: function (bankid) {
            $('#textBankName').val('');
            $.ajax({
                type: "post",
                url: _LocalhostURL + "/Ajax/GetBankList",
                dataType: "json",
                contentType: 'application/json; charset=utf-8',
                success: function (response) {
                    //debugger;
                    var select = $('#selectBank');
                    select.empty();
                    select.append('<option value="" selected >--請選擇--</option>');
                    $.each(response, function (index, item) {
                        var option = '';
                        if (bankid == item.BnakCode) {
                            option = '<option value="' + item.BnakCode + '" selected >' + item.Text + '</option>';
                            $('#textBankName').val(item.BankName);
                        }
                        else {
                            option = '<option value="' + item.BnakCode + '">' + item.Text + '</option>';
                        }
                        select.append(option);
                    });
                }
            });
        },
        //取得銀行分行list
        getBranchList: function (bankid, branchid) {
            if (bankid != null && branchid != null) {
                if (bankid == '700' && branchid.length == 7 && branchid != '7000021') {
                    branchid = branchid.slice(0, 6) + '-' + branchid.slice(6);
                }
            }
            $('#textBranchName').val('');
            var select = $('#selectBranch');
            select.empty();
            select.append('<option value="" selected >--請選擇--</option>');
            if (bankid != null && bankid != '') {
                select.prop('disabled', true);
                $.ajax({
                    type: "post",
                    url: _LocalhostURL + "/Ajax/GetBranchList",
                    dataType: "json",
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify({ 'code': bankid }),
                    success: function (response) {
                        //debugger;
                        $.each(response, function (index, item) {
                            var option = '';
                            if (bankid + branchid == item.BranchCode || branchid == item.BranchCode) {
                                option = '<option value="' + item.BranchCode + '" selected >' + item.Text + '</option>';
                                $('#textBankName').val(item.BankName)
                                $('#textBranchName').val(item.BranchName)
                            }
                            else {
                                option = '<option value="' + item.BranchCode + '">' + item.Text + '</option>';
                            }
                            select.append(option);
                        });
                        select.prop('disabled', false);
                    },
                    complete: function () {
                        //$.unblockUI();
                        select.prop('disabled', false);
                    },
                    error: function (xhr, status, error) {
                        //$.unblockUI();  debugger; console.error("錯誤:", error); alert("發生錯誤，請稍後再試");
                        select.prop('disabled', false);
                    }
                });
            }
        },
        checkCoUnit: function (coUnit) {
            if (coUnit == "" || coUnit == null) {
                alert("請填寫統一編號");
                return false;
            }
            else if (coUnit.toString().length != 8) {
                alert("統一編號必須為八碼");
                return false;
            }
            else if (!callback.self.config.commonController.fn.isValidGUI(coUnit)) {
                alert("統一編號格式錯誤!");
                return false;
            }
            return true;
        },
        // 圖片上傳
        uploadPhotoWithBoxes: function () {
            var fileInput = $('#fileInput')[0];
            if (!fileInput.files.length) {
                alert("請選擇要上傳的圖片");
                return;
            }

            const data = new FormData();
            data.append('epi', $('#Detail_epi').val());
            data.append('epb', $('#Detail_epb').val());
            data.append('ept', $('#Detail_ept').val());
            data.append('file', fileInput.files[0]);
            //data.append('note', '存摺');
            data.append('frontEndImageWidth', imgProps.naturalWidth);
            data.append('frontEndImageHeight', imgProps.naturalHeight);
            data.append("scanMode", state.scanType);

            boxes.forEach((box, index) => {
                data.append(`boxX${index + 1}`, box.x);
                data.append(`boxY${index + 1}`, box.y);
                data.append(`boxWidth${index + 1}`, box.width);
                data.append(`boxHeight${index + 1}`, box.height);
            });
            $.blockUI({ message: '<div>掃描中…</div>' });
            $.ajax({
                type: "POST",
                url: _LocalhostURL + "/EnterType/UploadAndParseImage",
                data: data,
                processData: false,
                contentType: false,
                success: function (ret) {
                    //debugger; // Json(new { result = new { Success = success, Data = ocrResults, Message = message, } });
                    if (ret == undefined || ret.result == undefined) { var msg1 = "上傳失敗!"; alert(msg1); }
                    if (ret.result && ret.result.Success) {
                        let txtBackImgFromScan1 = ret.result.Message;
                        $("#txtBackImgFromScan1").text('上傳結果: ' + txtBackImgFromScan1);

                        let bankList = $('#selectBank option').map(function () {
                            return { value: $(this).val(), text: $(this).text() };
                        }).get().filter(t => t.value);
                        let branchList = $('#selectBranch option').map(function () {
                            return { value: $(this).val(), text: $(this).text() };
                        }).get().filter(t => t.value);

                        let finAccount = '';
                        let finCode = '';
                        let finBranchCode = '';
                        let txtBankAccountFromScan = '';
                        let txtBankNameFromScan = '';
                        let txtBranchNameFromScan = '';
                        if (state.scanType === 'precise') {
                            finAccount = ret.result.Data.length > 0 ? ret.result.Data[0] : '';
                            finCode = ret.result.Data.length > 1 ? ret.result.Data[1] : '';
                            finBranchCode = ret.result.Data.length > 2 ? ret.result.Data[2] : '';
                            txtBankAccountFromScan = ret.result.Data.length > 3 ? ret.result.Data[3] : '';
                            txtBankNameFromScan = ret.result.Data.length > 4 ? ret.result.Data[4] : '';
                            txtBranchNameFromScan = ret.result.Data.length > 5 ? ret.result.Data[5] : '';
                        }
                        else {
                            var fullInfo = ret.result.Data.length > 0 ? ret.result.Data[0] : "";
                            document.getElementById("fullPassbookInfo").textContent = fullInfo;
                            finCode = ret.result.Data.length > 1 ? ret.result.Data[1] : '';
                            finBranchCode = ret.result.Data.length > 2 ? ret.result.Data[2] : '';
                            finAccount = ret.result.Data.length > 3 ? ret.result.Data[3] : '';
                            txtBankNameFromScan = finCode;
                            txtBranchNameFromScan = finBranchCode;
                            txtBankAccountFromScan = finAccount;
                        }

                        let isFinCodeChange = false;
                        if (bankList.some(t => t.value === finCode)) {
                            // 設定銀行 // (這邊不能用callback.self.getBankList，因為會發非同步request然後才賦值給$("#selectBank")，有可能下面先抓了賦值前的$("#selectBank").val()去取分行列表)
                            isFinCodeChange = $("#selectBank").val() !== finCode;
                            $("#selectBank").val(finCode);
                        }
                        else {
                            txtBankNameFromScan += '(未更動)';
                        }
                        if (!finBranchCode && txtBranchNameFromScan && branchList.some(t => t.text.includes(txtBranchNameFromScan))) {
                            // 如果後端沒找到分行代碼有可能是分行有掃到但沒掃到銀行，在這裡尋找原本的銀行的分行中有沒有符合掃描到的分行文字
                            finBranchCode = branchList.find(t => t.text.includes(txtBranchNameFromScan)).value;
                        }
                        if ($("#selectBank").val() && finBranchCode) {
                            // 設定分行 // (這邊只能傳finBranchCode給callback.self.getBranchList去對$("#selectBranch")賦值，
                            // 因為會發非同步request去取分行列表然後才賦值給$("#selectBranch")，
                            // 如果先用callback.self.getBranchList取分行然後在這裡賦值，有可能實際上賦值的時候callback.self.getBranchList還沒取完)
                            callback.self.getBranchList($("#selectBank").val(), finBranchCode);
                        }
                        else if (isFinCodeChange) {
                            // 有變更銀行但沒分行的話，只抓取分行列表，不賦值給$("#selectBranch")
                            callback.self.getBranchList($("#selectBank").val());
                        }
                        else {
                            txtBranchNameFromScan += '(未更動)';
                        }
                        if (finAccount) {
                            //設定銀行帳號
                            $("#textBankAccount").val(finAccount);
                        }
                        else {
                            txtBankAccountFromScan += '(未更動)';
                        }
                        $("#txtBankAccountFromScan").text('掃描結果: ' + txtBankAccountFromScan);
                        $("#txtBankNameFromScan").text('掃描結果: ' + txtBankNameFromScan);
                        $("#txtBranchNameFromScan").text('掃描結果: ' + txtBranchNameFromScan);
                    }
                    else {
                        var msg1 = "上傳失敗"; if (ret.result.Message != '') { msg1 = ret.result.Message; } alert(msg1);
                        $("#txtBankAccountFromScan").text('');
                        $("#txtBankNameFromScan").text('');
                        $("#txtBranchNameFromScan").text('');
                    }
                },
                complete: function () {
                    $.unblockUI();
                },
                error: function (xhr, status, error) {
                    //$.unblockUI();  debugger;
                    console.error("上傳錯誤:", error);
                    alert("上傳發生錯誤，請稍後再試");
                }
            });
        }

    });
    return callback;
}
