(function ($) {
    $.fn.Huploadify = function (opts) {
        var itemTemp = '<div id="${fileID}" class="uploadify-queue-item"><span class="up_filename">${fileName}</span><span class="uploadbtn">上傳</span><span class="delfilebtn">刪除</span><br/><div class="uploadify-progress"><div class="uploadify-progress-bar"></div></div></div>';
        var defaults = {
            fileTypeExts: '',//允許上傳的檔案類型，格式'*.jpg;*.doc'
            uploader: '',//文件提交的地址
            auto: false,//是否開啟自動上傳

            multi: true,//是否允許選擇多個檔
            formData: null,//發送給服務端的參數，格式：{key1:value1,key2:value2}
            fileObjName: 'file',//在後端接受檔的參數名稱，如PHP中的$_FILES['file']
            fileSizeLimit: 2048,//允許上傳的檔大小，單位KB
            showUploadedPercent: true,//是否即時顯示上傳的百分比，如20%
            showUploadedSize: false,//是否即時顯示已上傳的檔大小，如1M/2M
            buttonText: '選擇檔案',//上傳按鈕上的文字
            removeTimeout: 1000,//上傳完成後進度條的消失時間
            itemTemplate: itemTemp,//上傳佇列顯示的範本
            onUploadStart: null,//上傳開始時的動作
            onUploadSuccess: null,//上傳成功的動作
            onUploadComplete: null,//上傳完成的動作
            onUploadError: null, //上傳失敗的動作
            onInit: null,//初始化時的動作
            onCancel: null//刪除掉某個檔後的回呼函數，可傳入參數file
        }

        var option = $.extend(defaults, opts);

        //將檔的單位由bytes轉換為KB或MB，若第二個參數指定為true，則永遠轉換為KB
        var formatFileSize = function (size, byKB) {
            if (size > 1024 * 1024 && !byKB) {
                size = (Math.round(size * 100 / (1024 * 1024)) / 100).toString() + 'MB';
            }
            else {
                size = (Math.round(size * 100 / 1024) / 100).toString() + 'KB';
            }
            return size;
        }
        //根據檔序號獲取檔
        var getFile = function (index, files) {
            for (var i = 0; i < files.length; i++) {
                if (files[i].index == index) {
                    return files[i];
                }
            }
            return false;
        }

        //將輸入的檔案類型字串轉化為陣列,原格式為*.jpg;*.png
        var getFileTypes = function (str) {
            var result = [];
            var arr1 = str.split(";");
            for (var i = 0, len = arr1.length; i < len; i++) {
                result.push(arr1[i].split(".").pop());
            }
            return result;
        }

        this.each(function () {
            var _this = $(this);
            //先添加上file按鈕和上傳清單
            var instanceNumber = $('.uploadify').length + 1;
            var inputStr = '<input id="select_btn_' + instanceNumber + '" class="selectbtn" style="display:none;" type="file" name="fileselect[]"';
            inputStr += option.multi ? ' multiple' : '';
            inputStr += ' accept="';
            inputStr += getFileTypes(option.fileTypeExts).join(",");
            inputStr += '"/>';
            inputStr += '<a id="file_upload_' + instanceNumber + '-button" href="javascript:void(0)" class="uploadify-button">';
            inputStr += option.buttonText;
            inputStr += '</a>';
            var uploadFileListStr = '<div id="file_upload_' + instanceNumber + '-queue" class="uploadify-queue"></div>';
            _this.append(inputStr + uploadFileListStr);


            //創建檔物件
            var fileObj = {
                fileInput: _this.find('.selectbtn'),				//html file控制項
                uploadFileList: _this.find('.uploadify-queue'),
                url: option.uploader,						//ajax地址
                fileFilter: [],					//過濾後的檔陣列
                filter: function (files) {		//選擇檔組的過濾方法
                    var arr = [];
                    var typeArray = getFileTypes(option.fileTypeExts);
                    if (typeArray.length > 0) {
                        for (var i = 0, len = files.length; i < len; i++) {
                            var thisFile = files[i];
                            if (parseInt(formatFileSize(thisFile.size, true)) > option.fileSizeLimit) {
                                blockAlert('文件' + thisFile.name + '大小超出限制！');
                                continue;
                            }
                            if ($.inArray(thisFile.name.split('.').pop(), typeArray) >= 0) {
                                arr.push(thisFile);
                            }
                            else {
                                blockAlert('文件' + thisFile.name + '類型不允許！');
                            }
                        }
                    }
                    return arr;
                },
                //檔選擇後
                onSelect: function (files) {
                    for (var i = 0, len = files.length; i < len; i++) {
                        var file = files[i];
                        //處理範本中使用的變數
                        var $html = $(option.itemTemplate.replace(/\${fileID}/g, 'fileupload_' + instanceNumber + '_' + file.index).replace(/\${fileName}/g, file.name).replace(/\${fileSize}/g, formatFileSize(file.size)).replace(/\${instanceID}/g, _this.attr('id')));
                        //如果是自動上傳，去掉上傳按鈕
                        if (option.auto) {
                            $html.find('.uploadbtn').remove();
                        }
                        this.uploadFileList.append($html);

                        //判斷是否顯示已上傳檔大小
                        if (option.showUploadedSize) {
                            var num = '<span class="progressnum"><span class="uploadedsize">0KB</span>/<span class="totalsize">${fileSize}</span></span>'.replace(/\${fileSize}/g, formatFileSize(file.size));
                            $html.find('.uploadify-progress').after(num);
                        }

                        //判斷是否顯示上傳百分比	
                        if (option.showUploadedPercent) {
                            var percentText = '<span class="up_percent">0%</span>';
                            $html.find('.uploadify-progress').after(percentText);
                        }
                        //判斷是否是自動上傳
                        if (option.auto) {
                            this.funUploadFile(file);
                        }
                        else {
                            //如果配置非自動上傳，綁定上傳事件
                            $html.find('.uploadbtn').on('click', (function (file) {
                                return function () { fileObj.funUploadFile(file); }
                            })(file));
                        }
                        //為刪除檔按鈕綁定刪除檔事件
                        $html.find('.delfilebtn').on('click', (function (file) {
                            return function () { fileObj.funDeleteFile(file.index); }
                        })(file));
                    }


                },
                onProgress: function (file, loaded, total) {
                    var eleProgress = _this.find('#fileupload_' + instanceNumber + '_' + file.index + ' .uploadify-progress');
                    var percent = (loaded / total * 100).toFixed(2) + '%';
                    if (option.showUploadedSize) {
                        eleProgress.nextAll('.progressnum .uploadedsize').text(formatFileSize(loaded));
                        eleProgress.nextAll('.progressnum .totalsize').text(formatFileSize(total));
                    }
                    if (option.showUploadedPercent) {
                        eleProgress.nextAll('.up_percent').text(percent);
                    }
                    eleProgress.children('.uploadify-progress-bar').css('width', percent);
                },		//檔上傳進度

                /* 開發參數和內置方法分界線 */

                //獲取選擇檔，file控制項
                funGetFiles: function (e) {
                    // 獲取檔清單物件
                    var files = e.target.files;
                    //繼續添加檔
                    files = this.filter(files);
                    for (var i = 0, len = files.length; i < len; i++) {
                        this.fileFilter.push(files[i]);
                    }
                    this.funDealFiles(files);
                    return this;
                },

                //選中文件的處理與回檔
                funDealFiles: function (files) {
                    var fileCount = _this.find('.uploadify-queue .uploadify-queue-item').length;//佇列中已經有的檔個數
                    for (var i = 0, len = files.length; i < len; i++) {
                        files[i].index = ++fileCount;
                        files[i].id = files[i].index;
                    }
                    //執行選擇回檔
                    this.onSelect(files);

                    return this;
                },

                //刪除對應的檔
                funDeleteFile: function (index) {
                    for (var i = 0, len = this.fileFilter.length; i < len; i++) {
                        var file = this.fileFilter[i];
                        if (file.index == index) {
                            this.fileFilter.splice(i, 1);
                            _this.find('#fileupload_' + instanceNumber + '_' + index).fadeOut();
                            option.onCancel && option.onCancel(file);
                            break;
                        }
                    }
                    return this;
                },

                //文件上傳
                funUploadFile: function (file) {
                    var xhr = false;
                    try {
                        xhr = new XMLHttpRequest();//嘗試創建 XMLHttpRequest 物件，除 IE 外的流覽器都支持這個方法。
                    } catch (e) {
                        xhr = ActiveXobject("Msxml12.XMLHTTP");//使用較新版本的 IE 創建 IE 相容的物件（Msxml2.XMLHTTP）。
                    }

                    if (xhr.upload) {
                        // 上傳中
                        xhr.upload.addEventListener("progress", function (e) {
                            fileObj.onProgress(file, e.loaded, e.total);
                        }, false);

                        // 檔上傳成功或是失敗
                        xhr.onreadystatechange = function (e) {
                            if (xhr.readyState == 4) {
                                if (xhr.status == 200) {
                                    //校正進度條和上傳比例的誤差
                                    var thisfile = _this.find('#fileupload_' + instanceNumber + '_' + file.index);
                                    thisfile.find('.uploadify-progress-bar').css('width', '100%');
                                    option.showUploadedSize && thisfile.find('.uploadedsize').text(thisfile.find('.totalsize').text());
                                    option.showUploadedPercent && thisfile.find('.up_percent').text('100%');

                                    option.onUploadSuccess && option.onUploadSuccess(file, xhr.responseText);
                                    //在指定的間隔時間後刪掉進度條
                                    setTimeout(function () {
                                        _this.find('#fileupload_' + instanceNumber + '_' + file.index).fadeOut();
                                    }, option.removeTimeout);
                                } else {
                                    option.onUploadError && option.onUploadError(file, xhr.responseText);
                                }
                                option.onUploadComplete && option.onUploadComplete(file, xhr.responseText);
                                //清除檔選擇框中的已有值
                                fileObj.fileInput.val('');
                            }
                        };

                        option.onUploadStart && option.onUploadStart();
                        // 開始上傳
                        xhr.open(option.method, this.url, true);
                        xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
                        var fd = new FormData();
                        fd.append(option.fileObjName, file);
                        if (option.formData) {
                            for (key in option.formData) {
                                fd.append(key, option.formData[key]);
                            }
                        }

                        xhr.send(fd);
                    }


                },

                init: function () {
                    //檔選擇控制項選擇
                    if (this.fileInput.length > 0) {
                        this.fileInput.change(function (e) {
                            fileObj.funGetFiles(e);
                        });
                    }

                    //點擊上傳按鈕時觸發file的click事件
                    _this.find('.uploadify-button').on('click', function () {
                        _this.find('.selectbtn').trigger('click');
                    });

                    option.onInit && option.onInit();
                }
            };

            //初始設定檔案對象
            fileObj.init();
        });
    }

})(jQuery)
