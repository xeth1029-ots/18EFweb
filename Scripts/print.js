
/* 
共用 查詢結果清單 列印功能, 傳入 結果清單表格的 table id ,
會由分頁表單 qryForm 中取得 actionUrl 及 rid 的值,
並自動載入全部分頁內容後, 以列印格式顯示全部資料
*/
function PrintGrid(tableId, preFilterHandler, NoPaging) {

    if(!NoPaging) {
        // check paging form
        var form = $("form[name=qryParms]");
        if (form.attr("name") != "qryParms") {
            blockAlert("找不到分頁資訊表單: form[name=qryParms]");
            return;
        }
        var url = form.attr("action");
        if (url == '') {
            url = window.location.href;
        }
        var totalPages = form.attr("data-total-pages");
        var parms = {};
        parms.rid = form.find("input[name=rid]").val();
        parms.p = 1;

        if (!parms.rid) {
            blockAlert("沒有資料, 請先進行查詢!");
            return;
        }
    }

    var container = $('div.print-container');
    var gridTable = $("table#" + tableId).clone();

    if ($(gridTable).attr("id") != tableId) {
        blockAlert("找不到資料表格, 請先進行查詢!");
        return;
    }

    // hide page content
    $(".quickline").hide();
    $(".header").hide();
    $(".main").hide();

    $(gridTable).attr("id", tableId + "_print");
    $(gridTable).removeClass("table");
    $(gridTable).addClass("grid");

    /* find and append 功能路徑名稱 */
    var path = document.createElement('div');
    $(path).addClass("quickline");
    var i = 0;
    $("div.quickline div:eq(0)").find("a").each(function () {
        if (i > 0) {
            $(path).append("<span> &gt;&gt; </span>");
        }
        $(path).append("<span>" + $(this).text() + "</span>");
        i++;
    });

    container.find("div.print-body").html("");
    container.find("div.print-body").append(path);

    container.removeClass("hide");

    /* button click binding */
    container.find("div.print-btn #btnCancelPrint").on("click", function () {
        container.addClass("hide");
        $(".quickline").show();
        $(".header").show();
        $(".main").show();
    });

    container.find("div.print-btn #btnPrintBody").on("click", function () {
        window.print();
    });

    container.find("div.print-btn #btnExportToDOC").hide();

    container.find("div.print-btn #btnExportToXLS").on("click", function () {
        var expName = container.find("div.print-body div.quickline span").last().text();
        var content = container.clone();
        content.find("div.print-btn").remove();

        ExportToEXCEL(content, expName);
    });


    if (NoPaging) {
        // 沒有分頁
        unblockUI();

        if (preFilterHandler != undefined) {
            preFilterHandler(gridTable);
        }

        FilterPrintContent(gridTable);

        container.find("div.print-body").append(gridTable);
    }
    else
    {
        // 分頁資料表格
        // clear tbody of the current page
        $(gridTable).find("tbody").html('');

        blockUI();

        // Ajax load all the data
        AjaxLoadGridAll(url, parms, totalPages, gridTable, function () {
            /* fire on AjaxLoadGridAll finished */

            unblockUI();

            if (preFilterHandler != undefined) {
                preFilterHandler(gridTable);
            }

            FilterPrintContent(gridTable);

            container.find("div.print-body").append(gridTable);

        });
    }

    container.find("div.print-body").append("<br/>");
}

function AjaxLoadGridAll(url, parms, totalPages, gridTable, finishCallback) {
    if (parms.p > totalPages) {
        finishCallback();
        return;
    }
    ajaxLoadMore(url, parms, function (data) {
        var tbody = $(gridTable).find("tbody");
        tbody.html(tbody.html() + data);
        parms.p++;
        AjaxLoadGridAll(url, parms, totalPages, gridTable, finishCallback);
    }, false, true);
}


/* 共用 明細表單 列印功能, 傳入 編輯表單 的 div id */
function PrintDetail(detailId, preFilterHandler) {

    $(".quickline").hide();
    $(".header").hide();
    $(".main").hide();

    var container = $('div.print-container');

    var detailTable = $("#" + detailId).clone();
    $(detailTable).attr("id", detailId + "_print");
    $(detailTable).addClass("detail");

    if (preFilterHandler != undefined) {
        preFilterHandler(detailTable);
    }

    FilterPrintContent(detailTable);

    /* find and append 功能路徑名稱 */
    var path = document.createElement('div');
    $(path).addClass("quickline");
    var i = 0;
    $("div.quickline div:eq(0)").find("a").each(function () {
        if (i > 0) {
            $(path).append("<span> &gt;&gt; </span>");
        }
        $(path).append("<span>" + $(this).text() + "</span>");
        i++;
    });

    container.find("div.print-body").html('');
    container.find("div.print-body").append(path);
    container.find("div.print-body").append(detailTable);
    container.removeClass("hide");

    container.find("div.print-btn #btnCancelPrint").on("click", function () {
        container.addClass("hide");
        $(".quickline").show();
        $(".header").show();
        $(".main").show();
    });

    container.find("div.print-btn #btnPrintBody").on("click", function () {
        window.print();
    });

    container.find("div.print-btn #btnExportToXLS").hide();

    container.find("div.print-btn #btnExportToDOC").on("click", function () {
        var expName = container.find("div.print-body div.quickline span").last().text();
        $(container).find("div.print-btn").hide();

        ExportToWORD(container, expName);

        $(container).find("div.print-btn").show();
    });
}

/*列印格式處理, 將一些表單 tag 拿掉, 只留下乾淨的內容*/
function FilterPrintContent(printContent) {
    /* remove any tag within th */
    $(printContent).find("th").each(function () {
        var text = $(this).text();
        $(this).html(text);
    });
    /* remove a (role=button) tag */
    $(printContent).find("a[role=button]").each(function () {
        $(this).remove();
    });
    /* remove button tag */
    $(printContent).find("button").each(function () {
        $(this).remove();
    });
    /* remove all tag with .hidden class */
    $(printContent).find(".hidden").each(function () {
        $(this).remove();
    });
    /* remove all tag with .hide class */
    $(printContent).find(".hide").each(function () {
        $(this).remove();
    });
    /* remove input (type=hidden)  tag */
    $(printContent).find("input[type=hidden]").each(function () {
        $(this).remove();
    });
    /* remove textarea tag */
    $(printContent).find("textarea").wrap("<span></span>");
    $(printContent).find("textarea").each(function () {
        var value = $(this).val().replace(/\n/g, "<br />\n");

        $(this).remove();
    });
    /* remove select tag */
    $(printContent).find("select").wrap("<span></span>");
    $(printContent).find("select").each(function () {
        var sel = $(this);
        var selVal = $(this).val();
        sel.find("option").each(function () {
            if ($(this).attr("value") == selVal) {
                // keep selected option's text
                sel.parent().append($(this).text());
            }
        });
        sel.remove();
    });
    /* remove checkbox group tag */
    $(printContent).find("div.checkbox").each(function () {
        var input = $(this).find("input[type=checkbox]");
        if ($(input).is(":checked")) {
            // clear all but label text of checked item
            var value = $(this).text();
            $(this).html(value);
        }
        else {
            $(this).remove();
        }
    });
    /* remove un-checked radio group tag */
    $(printContent).find("input[type=radio]").each(function () {
        if ($(this).is(":checked")) {
            // checked item, remove input but leave label text
            $(this).remove();
        }
        else {
            $(this).parent().remove();
        }
    });
    /* remove un-checked checkbox tag */
    $(printContent).find("input[type=checkbox]").each(function () {
        if ($(this).is(":checked")) {
            // checked item, remove input but leave label text
            $(this).remove();
        }
        else {
            $(this).parent().remove();
        }
    });
    /* remove all other input tags */
    $(printContent).find("input").wrap("<span></span>");
    $(printContent).find("input").each(function () {
        var value = $(this).val();

        $(this).remove();
    });

    /* remove bootstrape css class from div */
    $(printContent).find(".radio").removeClass("radio");
    $(printContent).find(".checkbox").removeClass("checkbox");

    /* remove bootstrape div.form-group */
    $(printContent).find("div.form-group").wrap("<span></span>");
    $(printContent).find("div.form-group").each(function () {
        var html = $(this).html();
        $(this).parent().append(html);
        $(this).remove();
    });

    /* remove all tags within td */
    /*
    $(printContent).find("td").each(function () {
        var checkboxItems = Array();
        $(this).find("div.checkbox").each(function () {
            checkboxItems.push($(this).text());
        });
        var text;
        if (checkboxItems.length > 0) {
            text = checkboxItems.join("<br/>");
        }
        else {
            text = $(this).text();
        }
        $(this).html(text);
    }); */
}

/* 配合 Export/Word 運作, 將列印內容轉存 Word 檔案 */
function ExportToWORD(printContainer, expName) {
    var content = $(printContainer).wrap('<div></div>').parent().html();
    var form = $("form[name=exportWordForm]");
    form.find("input[name=expName]").val(expName);
    form.find("input[name=expContent]").val(content);
    form.submit();
}
/* 配合 Export/Excel 運作, 將列印內容轉存 Excel 檔案 */
function ExportToEXCEL(printContainer, expName) {
    var content = $(printContainer).wrap('<div></div>').parent().html();
    var form = $("form[name=exportExcelForm]");
    form.find("input[name=expName]").val(expName);
    form.find("input[name=expContent]").val(content);
    form.submit();
}
