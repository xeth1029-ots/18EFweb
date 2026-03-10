var autocomplete_delay = 50;
var autocomplete_timer = null;
var autocomplete_currentRequest = null;
var autocomplete_select = null;
var autocomplete_input_prefix = "";

$(document).ready(function () {

    /* binding oninput */
    autocomplete_init();

});

// binding oninput
function autocomplete_init() {
    $(".autocomplete[autocomplete-bind=Y]").each(function (index) {
        var input = $(this);
        console.log("autocomplete binding input: " + input.attr("id"));
        input
            .attr("autocomplete", "off")
            .attr("autocomplete-bind", "Y")
            .prop('imeStart', false)
            .on('compositionstart', function (evt) {
                $(this).prop('imeStart', true);
                //console.log("compositionstart, imeStart: " + $(this).prop('imeStart'))
            })
            .on('compositionend', function (evt) {
                if (!evt) { event = window.evt; }
                var target = $(evt.target);

                $(this).prop('imeStart', false);
                console.log("compositionend, imeStart: " + $(this).prop('imeStart'))

                // 在 IE, Chrome 環境, compositionend event 之後不會再觸發 input event
                // 所以這裡要呼叫 autocomplete_dropdown
                autocomplete_dropdown(target);
            })
            .on("input", function (evt) {
                if (!evt) { event = window.evt; }
                var target = $(evt.target);

                console.log("input, imeStart: " + $(this).prop('imeStart'))
                if ($(this).prop('imeStart')) return; //中文輸入法輸入過程,忽略
                autocomplete_dropdown(target);
            })
            .on("blur", function (evt) {
                if (!evt) { event = window.evt; }
                var target = $(evt.target);
                autocomplete_input_blur(target);
            });
    });
}

function autocomplete_input_blur(target) {
    console.log("autocomplete autocomplete_input_blur(" + target.attr("id") + "): data-target=" + $(autocomplete_select).data('target'));
    if (target.attr("id") != $(autocomplete_select).data('target')) {
        $(autocomplete_select).hide();
    }
    else {
        setTimeout(function () {
            if (!$(autocomplete_select).is(':focus')) {
                $(autocomplete_select).hide();
            }
        }, 100);
    }
}

function autocomplete_select_blur(target) {
    console.log("autocomplete autocomplete_select_blur(" + target.attr("id") + "): data-target=" + $(autocomplete_select).data('target'));
    var inputId = $(autocomplete_select).data('target');
    setTimeout(function () {
        if (!$("#" + inputId).is(':focus')) {
            $(autocomplete_select).hide();
        }
    }, 100);
}

function autocomplete_select_change(target) {
    var inputId = $(autocomplete_select).data('target');
    var input = $("#" + inputId);
    input.val(autocomplete_input_prefix + $(autocomplete_select).val());

    /* 待確認: 無障礙網頁規範以鍵盤[上][下]操作時, 是否會觸發多次 onchange 事件 
    直接 hide autocomplete_select 是否會造成不符預期的操作行為? */

    $(autocomplete_select).hide();
}

function autocomplete_dropdown(target) {
    console.log("autocomplete_dropdown(" + target.attr("id") + "):" + target.offset().left + "," + target.offset().top + ":" + target.val());

    var dataKey = target.data("key");
    var key = target.val() != null ? target.val().trim() : "";
    if (key == "") {
        console.log("autocomplete_dropdown_reset: key == \"\"");
        autocomplete_dropdown_reset(target);
        return;
    }
    else if (dataKey == key || key.length == 1) {
        // input 用字沒有變化, 忽略
        console.log("autocomplete_dropdown: key '" + key + "' unchange, ignored");
        return;
    }
    target.data("key", key);  // 保留最後一次呼叫 autocomplete 的用字

    var tfs = " AND tf:[2 TO 999999]";
    var cat = target.data("cat");
    if (cat == undefined || cat == null || cat.trim() == "") {
        cat = "*";
    }
    else {
        /* 20200429, eric, 學校(SCHOOL)、科系(DEPTCD)、公司名稱(COMPNAME)、求職登記表的備註欄位(DOC_MEMO)、求職登記表作業-介紹卡記錄備註欄位(REPLAY)
           類類名稱(CJOB_NAME) proxy串接那邊預設會查詢tf:[xx TO xx]，這類資料沒有進行tf計算請帶tfS=-1才查的到資料 */
        if (cat == "SCHOOL" || cat == "DEPTCD"
            || cat == "COMPNAME" || cat == "DOC_MEMO"
            || cat == "REPLAY" || cat == "CJOB_NAME"
        ) {
            tfs = "&tfs=-1";
        }

        cat = "(" + cat + ")";
    }

    // 20200408, eric, 調整參數 key 格式, 以提升效能
    var q = "key:(" + key + "* *" + key + ") AND cat:" + cat;
    // 20200316, eric, 去除已標記刪除的項目
    q += " NOT artificial:1";
    q += tfs;

    // 20200408, eric, 加入關鍵字長度範圍限制(2~8), 以提升關鍵字品質 //var kw = "kwS=2&kwE=8";

    if (!autocomplete_timer) {
        clearTimeout(autocomplete_timer);
    }

    //var url_SolrApi = "https://job.taiwanjobs.gov.tw/SolrApi.aspx"; //SolrApi.aspx '/SolrApi.aspx'
    var url_SolrApi = "/Ajax/GetSolrApi"; //SolrApi.aspx '/SolrApi.aspx'
    autocomplete_timer = setTimeout(function () {
        console.log("Calling SolrApi");
        autocomplete_currentRequest = jQuery.ajax({
            type: 'POST',
            /*data: 'q=' + q + "&" + kw, You-Shen: 會影響效能,去除 kw 參數 */
            data: 'q=' + q,
            url: url_SolrApi,
            beforeSend: function () {
                if (autocomplete_currentRequest != null) {
                    autocomplete_currentRequest.abort();
                }
            },
            success: function (data) {
                // Success
                console.log('success: function (data)');
                //console.log(target);
                console.log(data);
                autocomplete_dropdown_refresh(target, data);
            },
            error: function (e) {
                // Error
                console.log('error: function (e)!');
                console.log(e);
            }
        });
    }, autocomplete_delay);
}

function autocomplete_dropdown_create(target) {
    /* 因為 autocomplete_dropdown 會設定 position: absoulte; 所以必須參照 target.position() 而非 target.offset() 
    var top = target.offset().top + target.outerHeight(true); var left = target.offset().left;
    var top = target.position().top + target.outerHeight(true); var left = target.position().left; */
    var top = target.position().top + target.outerHeight(true);
    var left = target.position().left;

    var width = target.outerWidth();
    if (!target.is('input') && width < 250) {
        width = 250;
    }
    console.log('target:');
    console.log(target);
    var isCreate = false;
    var dropdownId = $(target).attr("autocomplete_dropdown");
    if (dropdownId) {
        autocomplete_select = $("#" + dropdownId);
    }
    if (autocomplete_select == null || autocomplete_select.length == 0) {
        isCreate = true;
        autocomplete_select = document.createElement("SELECT");
        $(autocomplete_select).prop("data-create", 'Y');
    }
    $(autocomplete_select).removeClass("hide");

    if (isCreate) {
        $(autocomplete_select).css({
            'width': width + 'px',
            'position': 'absolute',
            'z-index': '99',
            'top': top + 'px',
            'left': left + 'px',
            'border': '1px solid #CCC',
            'border-radius': '3px',
            'box-shadow': '0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19)',
            'scrollbar-width': 'none',
            'margin-top': 0
        });
    }
    else {
        $(autocomplete_select).css({
            'width': width + 'px'
        });
    }

    $(autocomplete_select)
        .data('target', target.attr('id'))
        .attr('size', 10)
        .on('blur', function (evt) {
            if (!evt) { event = window.evt; }
            var target = $(evt.target);
            autocomplete_select_blur(target);
        })
        .on('change', function (evt) {
            if (!evt) { event = window.evt; }
            var target = $(evt.target);
            autocomplete_select_change(target);
        });

    if (isCreate) {
        //document.body.appendChild(autocomplete_select);
        $(autocomplete_select).insertAfter(target);
    }

    console.log("autocomplete_dropdown_create:");
    console.log(autocomplete_select);
}

function autocomplete_dropdown_reset(target) {
    if (autocomplete_select != null) {
        $(autocomplete_select).empty();

        var top = target.offset().top + target.outerHeight(true);
        var left = target.offset().left;

        var width = target.outerWidth();
        if (!target.is('input') && width < 250) {
            width = 250;
        }

        $(autocomplete_select)
            .data('target', target.attr('id'))
            .css({ 'width': width + 'px' })
            .attr('size', 10)
            .show();
    }
}

function autocomplete_dropdown_hide() {
    if (autocomplete_select != null) {
        $(autocomplete_select).hide();
    }
}

function autocomplete_dropdown_refresh(target, solrData) {
    if (autocomplete_select == null) {
        autocomplete_dropdown_create(target);
    }
    else {
        var inputId = $(autocomplete_select).data('target');
        if (inputId != target.attr("id")) {
            // 不同一個 dropdown, 重新建立
            if ($(autocomplete_select).data('create') == "Y") {
                // 從 DOM 中移除動態建立的 dropdown
                $(autocomplete_select).remove();
            }
            autocomplete_select = null;
            autocomplete_dropdown_create(target);
        }
        else {
            autocomplete_dropdown_reset(target);
        }
    }
    console.log('autocomplete_select:');
    console.log(autocomplete_select);

    if (solrData && solrData.response && solrData.response.docs) {
        var docs = solrData.response.docs;
        var selSize = docs.length;
        if (selSize < 2) {
            selSize = 2;
        }
        $(autocomplete_select).attr('size', selSize);
        console.log(docs);
        for (var i = 0; i < docs.length; i++) {
            var option = $("<option></option>").text(docs[i].key)
                .appendTo($(autocomplete_select));
        }
    }
}

/* autocomplete 輸入框「新增」按鈕的處理 */
function autocompleteInputAppend(sourceId, targetId) {
    var inputSource = $("input#" + sourceId);
    var inputTarget = $("textarea[id$='" + targetId + "']");
    if (!inputTarget.length) {
        inputTarget = $("input[id$='" + targetId + "']");
    }
    if (!inputTarget.length) {
        console.log("autocompleteInputAppend: targetId '" + targetId + "' not found.");
        return;
    }
    console.log(inputTarget);
    var txtTarget = inputTarget.val();
    txtTarget = (txtTarget != undefined) ? txtTarget.trim() : "";
    if (txtTarget.length > 0) {
        txtTarget += "，";
    }

    inputTarget.val(txtTarget + inputSource.val());
    inputSource.val("");
}
