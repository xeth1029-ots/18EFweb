
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

            //設定資料
            if (config.editData != null) {
                callback.self.setViewData(config.editData);
                //callback.self.getBankList(config.editData.BankID);
                //callback.self.getBranchList(config.editData.BankID, config.editData.BranchID);
                //$('input[data-control="number"]').trigger('change');
            }

            //圖片上傳（正面）
            $('#submitBtn').on('click', callback.self.uploadPhotoWithBoxes);

            //圖片上傳（反面）
            $('#submitBtn2').on('click', callback.self.uploadPhotoWithBoxes2);

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

        // 圖片上傳（正面）
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
                url: _LocalhostURL + "/EnterType/UploadAndParseImageF",
                data: data,
                processData: false,
                contentType: false,
                success: function (ret) {
                    //debugger; // Json(new { result = new { Success = success, Data = ocrResults, Message = message, } });
                    if (ret == undefined || ret.result == undefined) { var msg1 = "上傳失敗!"; alert(msg1); }
                    if (ret.result && ret.result.Success) {
                        let txtBackImgFromScan1 = ret.result.Message;
                        $("#txtBackImgFromScan1").text('上傳結果: ' + txtBackImgFromScan1);
                        //debugger;
                        let finAcid = '';
                        let finBdate = ''; let finBdateY = ''; let finBdateM = ''; let finBdateD = '';
                        let finIssuedate = ''; let IssuedateY = ''; let IssuedateM = ''; let IssuedateD = '';
                        let txtIssueplace = '';
                        //身分證字號/出生年月日/發證日期/發證地
                        if (state.scanType === 'precise') {
                            finAcid = ret.result.Data.length > 0 ? ret.result.Data[0] : '';
                            finBdate = ret.result.Data.length > 1 ? ret.result.Data[1] : '';
                            finIssuedate = ret.result.Data.length > 2 ? ret.result.Data[2] : '';
                            txtIssueplace = ret.result.Data.length > 3 ? ret.result.Data[3] : '';
                        }
                        else {
                            var fullInfo = ret.result.Data.length > 0 ? ret.result.Data[0] : "";
                            document.getElementById("fullPassbookInfo").textContent = fullInfo;
                            finAcid = ret.result.Data.length > 1 ? ret.result.Data[1] : '';
                            finBdate = ret.result.Data.length > 2 ? ret.result.Data[2] : '';
                            finIssuedate = ret.result.Data.length > 3 ? ret.result.Data[3] : '';
                            txtIssueplace = ret.result.Data.length > 4 ? ret.result.Data[4] : '';
                        }
                        if (finBdate != '') {
                            const regex = /\s*(\d+)\s*年\s*(\d+)\s*月\s*(\d+)\s*日/;
                            const matches = finBdate.match(regex);
                            if (matches != null && matches.length > 3) {
                                finBdateY = matches[1]; finBdateM = matches[2]; finBdateD = matches[3];
                            }
                        }
                        if (finIssuedate != '') {
                            const regex2 = /\s*(\d+)\s*年\s*(\d+)\s*月\s*(\d+)\s*日/;
                            const matches2 = finIssuedate.match(regex2);
                            if (matches2 != null && matches2.length > 3) {
                                IssuedateY = matches2[1]; IssuedateM = matches2[2]; IssuedateD = matches2[3];
                            }
                        }
                        //console.log('finAcid:' + finAcid); console.log('finBdate:' + finBdate); console.log('finBdateY:' + finBdateY); console.log('finBdateM:' + finBdateM); console.log('finBdateD:' + finBdateD);
                        //console.log('finIssuedate:' + finIssuedate); console.log('IssuedateY:' + IssuedateY); console.log('IssuedateM:' + IssuedateM); console.log('IssuedateD:' + IssuedateD);
                        //console.log('txtIssueplace:' + txtIssueplace);
                        $("#Detail_IDNO").val(finAcid);
                        $("#Detail_BIRTH_Y").val(finBdateY); $("#Detail_BIRTH_M").val(finBdateM); $("#Detail_BIRTH_D").val(finBdateD);
                        $("#Detail_ISSUE_Y").val(IssuedateY); $("#Detail_ISSUE_M").val(IssuedateM); $("#Detail_ISSUE_D").val(IssuedateD);
                        $("#Detail_ISSUINGPLACE").val(txtIssueplace);
                    }
                    else {
                        var msg1 = "上傳失敗"; if (ret.result.Message != '') { msg1 = ret.result.Message; } alert(msg1);
                        document.getElementById("fullPassbookInfo").textContent = "";
                        $("#Detail_IDNO").val('');
                        $("#Detail_BIRTH_Y").val(''); $("#Detail_BIRTH_M").val(''); $("#Detail_BIRTH_D").val('');
                        $("#Detail_ISSUE_Y").val(''); $("#Detail_ISSUE_M").val(''); $("#Detail_ISSUE_D").val('');
                        $("#Detail_ISSUINGPLACE").val('');
                        //$("#txtBankAccountFromScan").text(''); //$("#txtBankNameFromScan").text(''); //$("#txtBranchNameFromScan").text('');
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
        ,
        // 圖片上傳（反面）
        uploadPhotoWithBoxes2: function () {
            var fileInput2 = $('#fileInput2')[0];
            if (!fileInput2.files.length) {
                alert("請選擇要上傳的圖片");
                return;
            }

            const data = new FormData();
            data.append('epi', $('#Detail_epi').val());
            data.append('epb', $('#Detail_epb').val());
            data.append('ept', $('#Detail_ept').val());
            data.append('file', fileInput2.files[0]);
            //data.append('note', '存摺');
            data.append('frontEndImageWidth', imgProps.naturalWidth);
            data.append('frontEndImageHeight', imgProps.naturalHeight);
            //data.append("scanMode", state.scanType);
            data.append("scanMode", "full");

            $.blockUI({ message: '<div>上傳中…</div>' });
            $.ajax({
                type: "POST",
                url: _LocalhostURL + "/EnterType/UploadAndParseImageB",
                data: data,
                processData: false,
                contentType: false,
                success: function (ret) {
                    //debugger; // Json(new { result = new { Success = success, Data = ocrResults, Message = message, } });
                    if (ret == undefined || ret.result == undefined) { var msg1 = "上傳失敗!"; alert(msg1); }
                    if (ret.result && ret.result.Success) {
                        let txtBackImgFromScan2 = ret.result.Message;
                        $("#txtBackImgFromScan2").text('上傳結果: ' + txtBackImgFromScan2);

                        var fullInfo2 = ret.result.Data.length > 0 ? ret.result.Data[0] : "";
                        document.getElementById("fullPassbookInfo2").textContent = fullInfo2;
                        //$("#txtBankAccountFromScan").text('掃描結果: ' + txtBankAccountFromScan);
                        //$("#txtBankNameFromScan").text('掃描結果: ' + txtBankNameFromScan);
                        //$("#txtBranchNameFromScan").text('掃描結果: ' + txtBranchNameFromScan);
                    }
                    else {
                        var msg1 = "上傳失敗"; if (ret.result.Message != '') { msg1 = ret.result.Message; } alert(msg1);
                        document.getElementById("fullPassbookInfo2").textContent = "";
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
