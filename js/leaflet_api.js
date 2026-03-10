Object(
    leafletApi = {
        mapHeight: "",
        mapWidth: "",
        mapSetting: {
            zoom: 7.5,  // 全台灣範圍最大化顯示
            center: [23.750991850863706, 120.4907224327326],
            maxZoom: 18,
            minZoom: 7,
            zoomSnap: 0  // 呼叫 fitBounds 時, 不要自動將 zoom 值趨近為整數值
        },
        layerSetting: {
            url: "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            options: {
                attribution: "&copy; <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributor"
            }
        },

        map: null,
        tileLayer: null,
        curFocusedMarker: null,
        mapSize: function () {
            var t = {};
            return this.mapHeight && (t.height = this.mapHeight), this.mapWidth && (t.width = this.mapWidth), t
        },
        initMap: function (mapDivId, mapReady) {
            L.DomUtil.TRANSITION = "webkitTransition";

            if ($(window).width() <= 480) {
                // 手機 RWD 顯示, 微調整 Map 初始參數
                this.mapSetting.center = [23.800752336349056, 120.73013120913306];
                this.mapSetting.zoom = 7.5;
            }

            this.map = L.map(mapDivId, this.mapSetting);
            this.tileLayer = L.tileLayer(this.layerSetting.url, this.layerSetting.options);
            this.tileLayer.addTo(this.map);
            this.map.invalidateSize();
            this.map.attributionControl.setPosition("bottomleft");
            this.bindMapEvent();
            if (typeof mapReady == "function") {
                mapReady(this.map);
            }
        },
        bindMapEvent: function () {
            var t = this;
            if (this.map) {
                var wn = Object.keys(mapEventHandler);
                wn.forEach((function (e) {
                    t.map.on(e, (function (o) {
                        if (mapEventHandler.hasOwnProperty(e)) {
                            // trigger event handler
                            mapEventHandler[e](o);
                        }
                    }))
                }));
            }
        },

        /*
        取得地圖顯示區域 bounds {left, top, right, bottom}
        */
        getMapBounds: function () {
            var bounds = this.map.getBounds();
            return {
                left: bounds._southWest.lng,
                bottom: bounds._southWest.lat,
                right: bounds._northEast.lng,
                top: bounds._northEast.lat
            };
        },


        getZoom: function () {
            return this.map.getZoom();
        },

        setZoom: function (zoom) {
            this.map.setZoom(zoom);
        },

        /* 
          設定地圖中心點, p [WGS84座標-Y, WGS84座標-X]
          並可以同時進行縮放(選項) 
        */
        setCenter: function (p, zoom) {
            this.map.flyTo(p, zoom);
        },

        /*
          在指定的座標顯示單一標記點位(icon 以 div 格式建立):
          p: [WGS84座標-Y, WGS84座標-X],
          markerSize: 標記點位的直徑(px), 預設為 40px,
          title: 標記點的 title 屬性,
          iconClass: 標記點位套用的 CSS class, 預設值: map-marker-focused,
          iconText: 點中要顯示的文字
        */
        setSingleMarker(p, markerSize, title, iconClass, iconText) {
            console.log(Log("setSingleMarker"));

            if (!markerSize || markerSize == undefined) {
                markerSize = 40;
            }
            if (title == undefined) {
                title = "";
            }
            if (iconClass == undefined) {
                iconClass = "map-marker-focused";
            }
            if (iconText == undefined) {
                iconText = "";
            }
            var lineHeight = markerSize - 4;  // 扣除 border-width

            // 檢查 curFocusedMarker 是否已存在, 若存在則先移除
            // 地圖中只允許一個 User Focused Marker
            if (this.curFocusedMarker != null && this.map.hasLayer(this.curFocusedMarker)) {
                this.map.removeLayer(this.curFocusedMarker);
            }

            var customIcon = L.divIcon({
                className: 'map-marker ' + iconClass,
                html: "<div title=\"" + title + "\" style=\"line-height:" + lineHeight + "px\">" + iconText + "</div>",
                iconSize: [markerSize, markerSize],
                iconAnchor: [markerSize / 2, markerSize]
            });
            this.curFocusedMarker = L.marker(
                    p,
                    { icon: customIcon }
                );

            this.map.addLayer(this.curFocusedMarker);
        },

        /* 
        傳入 bounds 物件, 讓地圖縮放顯示區域符合指定的 bounds {left, top, right, bottom} 
        傳人的座標必須為 WGS84 (EPSG4326) 座標
        */
        setMapBounds: function (bounds) {
            console.log("setMapBounds: " + JSON.stringify(bounds));

            if (!bounds || bounds == 'undefined') {
                console.error("setMapBounds: bounds is undefined");
                return;
            }

            var corner1 = L.latLng(bounds.top, bounds.left);
            var corner2 = L.latLng(bounds.bottom, bounds.right);
            var lBounds = L.latLngBounds(corner1, corner2);

            this.map.flyToBounds(lBounds);
        },

        /* 
        建立標記點圖層, 
        傳入一個 markerDatas 陣列, 每一個元素為 object:
        {
            id: 點位ID,
            name: 點位名稱,
            locX: WGS84座標-X,
            locY: WGS84座標-Y,
            class: 點位指定的 css class [optional],
            onClick: 標記點click事件 handle function,
            data: 原始資料
        },
        onClick: 標記點click會觸發的 callback,
        markerSize: 標記直徑(px), 預設為40px
        numberText: 要顯示標記點位數字後面的單位文字, 預設為空白
        fitBounds: 是否自動縮放地圖以符合點位座標資料範圍, 預設為 false
        */
        setMarkers: function (markerDatas, onMarkerClick, markerSize, numberText, fitBounds) {
            //console.log(Log("setMarkers"));
            //var g_ctname ="";

            if (!markerSize || markerSize == undefined) {
                markerSize = 40;
            }
            if (numberText == undefined) {
                numberText = "";
            }

            // 若 ClusterLayer 已存在, 先移除
            if (this.ClusterGroupLayer) {
                if (this.map.hasLayer(this.ClusterGroupLayer)) {
                    this.map.removeLayer(this.ClusterGroupLayer);
                    this.ClusterGroupLayer = undefined;
                }
            }

            // 建立 ClusterGroupLayer
            this.ClusterGroupLayer = L.markerClusterGroup({
                maxClusterRadius: 60,
                showCoverageOnHover: false,
                zoomToBoundsOnClick: false,
                singleMarkerMode: true,
                spiderfyOnMaxZoom: false,
                animate: true,
                //animateAddingMarkers: true,
                iconCreateFunction: function (cluster) {
                    //console.log("cluster iconCreateFunction");
                    //console.log(cluster);

                    cluster.addIconClass = function (c) {
                        $(this._icon).addClass(c);
                    }
                    cluster.removeIconClass = function (c) {
                        $(this._icon).removeClass(c);
                    }

                    var number = cluster.getChildCount();
                    var iconClass = "custom-cluster";
                    var iconCtnmae = null;
                    if (number > 1) {
                        var markers = cluster.getAllChildMarkers();
                        if (markers.length > 0) {
                            if (markers[0].itemCtnmae) { iconCtnmae = markers[0].itemCtnmae; }
                        }
                    }
                    else if (number == 1) {
                        //console.log("single cluster iconCreateFunction");
                        //單一點位 cluster, 判斷點位是否有指定 itemClass, 若有則套用
                        var markers = cluster.getAllChildMarkers();
                        if (markers.length > 0) {
                            if (markers[0].itemClass) { iconClass = markers[0].itemClass; }
                            if (markers[0].itemCtnmae) { iconCtnmae = markers[0].itemCtnmae; }
                        }
                    }

                    // 依群聚點合併數量, 計算/調整圖型大小
                    var n = number, digits = 0;
                    while (n / 10 >= 1) {
                        n /= 10;
                        digits++;
                    }
                    digits = (digits > 1) ? digits - 1 : 0;
                    var d = markerSize + (markerSize * digits * 0.2);
                    var lineHeight = d - 4;  // 扣除 border-width

                    let l_num = number + numberText
                    let s_html = "<div style='line-height:" + lineHeight + "px'>" + l_num + "</div>";
                    if (iconCtnmae != undefined && iconCtnmae != null) {
                        let l_title = iconCtnmae + "附近";
                        let l_title2 = "課程有" + l_num + l_title;
                        s_html = "<div style='line-height:" + lineHeight + "px' aria-label=\"" + l_title2 + "\" title=\"" + l_title2 + "\">" + l_num + "<span class=\"sr-only\">" + l_title + "</span></div>";
                    }
                    return L.divIcon({
                        html: s_html,
                        className: 'cluster ' + iconClass,
                        iconSize: [d, d],
                        iconAnchor: [d / 2, d]
                    });
                }
            });

            // 用來記錄點位分佈 bounds
            var markerBounds = {
                'left': 123,
                'top': 21,
                'right': 117,
                'bottom': 26
            };

            // 建立點位並加入到 ClusterGroupLayer
            for (var i = 0; i < markerDatas.length; i++) {
                var item = markerDatas[i];
                let ctname = item.ctname;
                //g_ctname = item.ctname;
                markerBounds.left = item.locX < markerBounds.left ? item.locX : markerBounds.left;
                markerBounds.top = item.locY > markerBounds.top ? item.locY : markerBounds.top;
                markerBounds.right = item.locX > markerBounds.right ? item.locX : markerBounds.right;
                markerBounds.bottom = item.locY < markerBounds.bottom ? item.locY : markerBounds.bottom;

                var marker = L.marker(
                    new L.LatLng(item.locY, item.locX),
                    L.divIcon({
                        className: 'marker ' + (item.class != 'undefined' ? item.class : ''),
                        html: "<div></div>"
                    })
                    );

                marker.itemId = item.id;
                marker.itemName = item.name;
                marker.itemClass = item.class;
                marker.itemCtnmae = item.ctname;

                marker.addIconClass = function (c) {
                    $(this._icon).addClass(c);
                }
                marker.removeIconClass = function (c) {
                    $(this._icon).removeClass(c);
                }

                // marker click 事件
                marker.on("click", function (a) {
                    console.log('singleMark click');
                    //console.log(a);

                    $(".custom-cluster-focused").each(function (e) {
                        $(this).removeClass("custom-cluster-focused");
                    });

                    $(a.target._icon).addClass("custom-cluster-focused");

                    if (typeof onMarkerClick == "function") {
                        onMarkerClick([a.target]);
                    }
                });

                this.ClusterGroupLayer.addLayer(marker);
            }

            // cluster click 事件
            this.ClusterGroupLayer.on('clusterclick', function (a) {
                console.log('clusterclick');
                //console.log(a);

                $(".custom-cluster-focused").each(function (e) {
                    $(this).removeClass("custom-cluster-focused");
                });

                // a.layer is actually a cluster
                $(a.layer._icon).addClass("custom-cluster-focused");

                if (typeof onMarkerClick == "function") {
                    onMarkerClick(a.layer.getAllChildMarkers());
                }
            });


            // 加到地圖中
            this.map.addLayer(this.ClusterGroupLayer);

            if (fitBounds) {
                markerBounds.left -= 0.1;
                markerBounds.right += 0.1;
                markerBounds.top += 0.1;
                markerBounds.bottom -= 0.1;

                this.setMapBounds(markerBounds);
            }

            console.log(Log(this.ClusterGroupLayer));
        },

        /* 
          將指定的 marker 標示成 focused
        */
        focusMark: function (m) {
            $(m._icon).addClass("custom-cluster-focused");
        },

        /*
        搜尋包含指定 itemId Marker 的 ClusterGroup Markers,
        回傳 undefined 表示找不到, null 表示不在當前地圖顯示範圍
        */
        findMarkerByItemId: function (itemId) {
            var marker;
            this.ClusterGroupLayer.eachLayer(function (m) {
                // 每一個已加入到 ClusterGroupLayer 的單一點位 marker
                if (m.itemId && m.itemId == itemId) {
                    marker = m;
                    return false;
                }
            });

            var visibleOne;
            if (marker != undefined) {
                visibleOne = this.ClusterGroupLayer.getVisibleParent(marker);
            }

            return visibleOne;
        },

        /* 
        依傳入的 geoJson Polygon 資料, 以指定 style 進行地理區域的標示 
        若 pStyle 沒有指定, 預設為:
        {
          "color": "#0078ff",
          "weight": 3,
          "opacity": 0.65
        }
        */
        geoFill: function (geoJson, pStyle) {
            if (!pStyle) {
                pStyle = {
                    "color": "#0078ff",
                    "weight": 3,
                    "opacity": 0.65
                };
            }
            L.geoJSON(geoJson, {
                style: pStyle
            }).addTo(this.map);
        }
    },

    /* 地圖事件處理介面 */
    mapEventHandler = {
        mousedown: function (evt) {
            //console.log(evt);
        },
        dragstart: function (evt) {
            //console.log(evt);
        },
        dragend: function (evt) {
            console.log(evt);
        },
        click: function (evt) {
            console.log(evt);
        },
        zoom: function (evt) {
            //console.log(evt);
        },
        zoomstart: function (evt) {
            //console.log(evt);
        },
        zoomend: function (evt) {
            console.log(evt);
        },
        movestart: function (evt) {
            //console.log(evt);
        }
    }
);

function Log(msg) {
    if (typeof msg == "string") {
        let now = new Date();
        let t = "[" + now.getMinutes() + ":" + now.getSeconds() + "." + now.getMilliseconds() + "] ";
        msg = t + msg;
    }
    return msg;
}



