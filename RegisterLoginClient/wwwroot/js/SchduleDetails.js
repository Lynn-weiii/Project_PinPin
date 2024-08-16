

//#region 加載 Google Maps API
(function (g) {
    var h, a, k, p = "The Google Maps JavaScript API",
        c = "google",
        l = "importLibrary",
        q = "__ib__",
        m = document,
        b = window;
    b = b[c] || (b[c] = {});
    var d = b.maps || (b.maps = {}),
        r = new Set(),
        e = new URLSearchParams(),
        u = () => h || (h = new Promise(async (f, n) => {
            a = m.createElement("script");
            e.set("libraries", [...r].join(","));
            for (k in g) e.set(k.replace(/[A-Z]/g, t => "_" + t[0].toLowerCase()), g[k]);
            e.set("callback", c + ".maps." + q);
            e.set("language", "zh-TW");
            e.set("region", "TW");
            a.src = `https://maps.${c}apis.com/maps/api/js?${e}`;
            d[q] = f;
            a.onerror = () => h = n(Error(p + " could not load."));
            a.nonce = m.querySelector("script[nonce]")?.nonce || "";
            m.head.append(a);
        }));
    d[l] ? console.warn(p + " only loads once. Ignoring:", g) : d[l] = (f, ...n) => r.add(f) && u().then(() => d[l](f, ...n));
})({
    key: "AIzaSyCUl-h1ooBble_5ATQVVSjSJL0O2F6DHAo",
    v: "weekly",
    libraries: "places,geometry,drawing,visualization"
});
//#endregion

//#region 初始化
async function LoadScheduleInfo(scheduleId) {
    try {
        const response = await fetch(`${baseAddress}/api/Schedules/Entereditdetailsch/${scheduleId}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const results = await response.json();
            const scheduleDateIdInfo = results.sceduleDateIdInfo; // 修正键名
            const scheduleDetail = results.scheduleDetail;
            //scheduleDateIdInfo存在sessionstorge

            const datesArray = Array.isArray(scheduleDateIdInfo) ? scheduleDateIdInfo : [scheduleDateIdInfo];

            const allDetails = []; // 存储所有详细信息的数组

            let picture, startTime, endTime, name, parsedLat, parsedLng, placeId, caneditdetail, canedittitle;

            if (scheduleDetail) {
                const detail = Array.isArray(scheduleDetail) ? scheduleDetail[0] : scheduleDetail;

                name = detail.name;
                lat = detail.lat;
                lng = detail.lng;
                placeId = detail.placeId;
                caneditdetail = detail.caneditdetail;
                canedittitle = detail.canedittitle;
                startTime = detail.startTime;
                endTime = detail.endTime;

                parsedLat = parseFloat(lat);
                parsedLng = parseFloat(lng);

                if (isNaN(parsedLat) || isNaN(parsedLng)) {
                    console.error('Invalid coordinates:', { lat, lng });
                    alert('Invalid location coordinates received.');
                    return;
                }
                picture = await fetchPlacePhotoUrl(placeId);
                var data = {
                    name: name,
                    lat: parsedLat,
                    lng: parsedLng,
                    placeId: placeId,
                    caneditdetail: caneditdetail,  
                    canedittitle: canedittitle,
                    startTime: startTime,
                    endTime: endTime,
                    picture: picture,
                    placeId: placeId,
                    scheduleId: scheduleId
                }

                generateTabs(data, scheduleDateIdInfo);

                if (document.getElementById('theme-header')) {
                    document.getElementById('theme-header').style.backgroundImage = `url('${picture}')`;
                }

                const travelduring = document.getElementById('travelduring');
                if (travelduring) {
                    travelduring.innerHTML = `${startTime} <i class="fa-solid fa-arrow-right"></i> ${endTime}`;
                }

                if (document.getElementById('theme-name')) {
                    document.getElementById('theme-name').innerText = name;
                }

                const position = { lat: parsedLat, lng: parsedLng };
                initMap(scheduleId, name, position, placeId);

                if (canedittitle) {
                    //#region 主揪可以編輯日程時間的按鈕
                    /* $('#schtitle_edit_btn').show();*/
                   //#endregion
                } else {
                    console.log('canedittitle is false');
                }
            } else {
                console.error('scheduleDetail is undefined or invalid:', scheduleDetail);
                alert('Received invalid schedule detail data.');
            }
        } else {
            console.error('Failed to fetch schedule info:', response.statusText);
        }
    } catch (error) {
        console.error('Error fetching schedule info:', error);
    }
}


    async function fetchPlacePhotoUrl(placeId) {
        const { PlacesService } = await google.maps.importLibrary("places");
        const map = new google.maps.Map(document.createElement('div'));
        const placesService = new PlacesService(map);

        return new Promise((resolve, reject) => {
            const request = {
                placeId: placeId,
                fields: ['photos']
            };

            placesService.getDetails(request, (place, status) => {
                if (status === google.maps.places.PlacesServiceStatus.OK) {
                    if (place.photos && place.photos.length > 0) {
                        const picture = place.photos[0].getUrl();
                        resolve(picture); // 返回照片 URL
                    } else {
                        resolve(null); // 没有照片时返回 null
                    }
                } else {
                    reject('Error fetching place details: ' + status);
                }
            });
        });
    }
    function handleResponseErrors(response) {
    switch (response.status) {
        case 204:
            alert('無法找到相關日程資訊!');
            break;
        case 401:
            alert('請重新登入再使用功能!');
            goToLoginPage();
            break;
        default:
            response.json().then(errorResult => {
                alert(errorResult.message || '發生錯誤');
            });
            break;
    }
}
//#endregion

//#region google API 
let map, marker, autocomplete, infowindow, position, service, keyword, searchTimeout, idleListener,globalName,globalScheduleId;
let markers = []; 
async function initMap(scheduleId, name, position,placeId) {
    console.log('initMap:', { scheduleId, name, position });
    globalName = name;
    globalScheduleId = scheduleId;
    const { Map } = await google.maps.importLibrary("maps");
    const { AdvancedMarkerElement } = await google.maps.importLibrary("marker");
    const { Geocoder } = await google.maps.importLibrary("geocoding");
    var placeId = placeId;
    map = new Map(document.getElementById("map"), {
        center: position,
        zoom: 14,
        mapId: "d4432686758d8acc",
    });

    infowindow = new google.maps.InfoWindow();
    geocoder = new Geocoder();

    service = new google.maps.places.PlacesService(map);
    
    const autocomplete = new google.maps.places.Autocomplete(document.getElementById('search_input_field'));
    autocomplete.bindTo('bounds', map);

    autocomplete.addListener('place_changed', function () {
        infowindow.close();
        clearMarkers();

        const place = autocomplete.getPlace();
        if (!place.geometry) {
            console.log("Autocomplete's returned place contains no geometry");
            return;
        }

        if (place.geometry.viewport) {
            map.fitBounds(place.geometry.viewport);
        } else {
            map.setCenter(place.geometry.location);
            map.setZoom(13);
        }

        const marker = new AdvancedMarkerElement({
            position: place.geometry.location,
            map: map,
            title: place.name,
        });
        marker.placeInfo = { name, scheduleId }; // 存储 name 和 scheduleId 到标记对象
        markers.push(marker);

        console.log('Autocomplete place selected:', { place, name, scheduleId });
        const content = createInfoWindowContent(place, name, scheduleId);
        infowindow.setContent(content);
        infowindow.open(map, marker);

        // 移除旧的监听器，防止重复监听
        document.querySelector('#add-place-btn').removeEventListener('click', addPlaceToScheduleHandler);

        // 添加新的监听器
        document.querySelector('#add-place-btn').addEventListener('click', addPlaceToScheduleHandler);

        const keyword = document.getElementById('search_input_field').value;
        performNearbySearch(place.geometry.location, keyword, name, scheduleId);
    });

    keyword = document.getElementById('search_input_field').addEventListener('keyup', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            const keyword = this.value;
            performNearbySearch(map.getCenter(), keyword, name, scheduleId);
        }
    });

    idleListener = map.addListener('idle', function () {
        const keyword = document.getElementById('search_input_field').value;
        if (keyword) {
            performNearbySearch(map.getCenter(), keyword, name, scheduleId);
        }
    });

    // 添加地图点击事件监听器
    map.addListener('click', function (event) {
        console.log('Map clicked:', { lat: event.latLng.lat(), lng: event.latLng.lng(), name, scheduleId });
        infowindow.close(); 
        const content = createInfoWindowContent({ name: '', geometry: { location: event.latLng } }, name, scheduleId);
        infowindow.setContent(content);
        infowindow.setPosition(event.latLng);
        infowindow.open(map);

        // 在创建 InfoWindow 后，调用 geocodeLatLng 获取更多信息
        geocodeLatLng(event.latLng);
    });
}
function clearMarkers() {
    markers.forEach((marker) => {
        if (marker) {
            marker.map = null;
        }
    });
    markers = [];
}
async function performNearbySearch(location, keyword, name, scheduleId) {    
    const { AdvancedMarkerElement } = await google.maps.importLibrary("marker");
    const request = {
        location: location,
        radius: 2000,
        keyword: keyword
    };

    if (idleListener) {
        google.maps.event.removeListener(idleListener);
    }

    service.nearbySearch(request, function (results, status) {
        if (status === google.maps.places.PlacesServiceStatus.OK) {
            clearMarkers();

            const filteredResults = results.filter(place => {
                const distance = google.maps.geometry.spherical.computeDistanceBetween(location, place.geometry.location);
                return distance <= 2000;
            });

            const displayResults = filteredResults.length > 0 ? filteredResults : results;

            displayResults.forEach((place) => {
                const marker = new AdvancedMarkerElement({
                    map: map,
                    position: place.geometry.location,
                    title: place.name,
                });
                marker.placeInfo = { name, scheduleId }; 
                markers.push(marker);

                google.maps.event.addListener(marker, 'click', function () {
                    infowindow.setContent(createInfoWindowContent(place, name, scheduleId));
                    infowindow.open(map, marker);

                    //document.querySelector('#add-place-btn').addEventListener('click', function () {
                    //    addPlaceToSchedule(place.geometry.location.lat(), place.geometry.location.lng(), place.place_id, scheduleId);
                    //});
                });
            });

            if (displayResults.length > 0) {
                map.setCenter(displayResults[0].geometry.location);
                map.setZoom(16);
            }
        } else {
            console.error('Nearby Search failed:', status);
            Swal.fire({
                title: "Error",
                text: '找不到相關地點。請重新輸入。',
                icon: 'error'
            });
        }

        $('#search_input_field').val('');
    });
}
async function geocodeLatLng(latlng) {
    const name = globalName;
    const scheduleId = globalScheduleId;
    console.log('geocodeLatLng called with:', { latlng, name, scheduleId });
    const { AdvancedMarkerElement } = await google.maps.importLibrary("marker");
    geocoder.geocode({ location: latlng }, (results, status) => {
        if (status === "OK") {
            if (results[0]) {
                const placeId = results[0].place_id;
                if (placeId) {
                    service.getDetails({ placeId: placeId }, async function (place, status) {
                        if (status === google.maps.places.PlacesServiceStatus.OK) {
                            clearMarkers();
                            const marker = new AdvancedMarkerElement({
                                position: latlng,
                                map: map
                            });
                            marker.placeInfo = { name, scheduleId }; 
                            markers.push(marker);

                            const content = createInfoWindowContent(place, name, scheduleId);
                            infowindow.setContent(content);
                            infowindow.open(map, marker);


                        } else {
                            window.alert("No results found");
                        }
                    });
                }
            }
        } else {
            window.alert("Geocoder failed due to: " + status);
        }
    });
}
function createInfoWindowContent(place, name, scheduleId) {
    let content = '<div class="info-window">';

    if (place.photos && place.photos.length > 0) {
        content += `<img src="${place.photos[0].getUrl({ maxWidth: 280 })}" alt="${place.name}" class="info-window-photo"><br>`;
    }

    content += `<div class="details"><strong>${place.name}</strong></div>`;
    content += `<div class="rating-container">
                    <div class="rating">
                        <span class="average-rating">${place.rating || 'N/A'}</span>
                        ${place.rating ? getStarRating(place.rating) : ''}
                    </div>
                    <div class="reviews">(${place.user_ratings_total || 0} 則評價)</div>
                </div>`;
    content += `<div class="address">${place.vicinity || place.formatted_address}</div>`;
    content += `<div class="btn-container">
                    <button class="btn btn-primary" id="add-place-btn" data-id="${scheduleId}" onclick="addPlaceToSchedule('${place.geometry.location.lat()}', '${place.geometry.location.lng()}', '${place.place_id}', '${scheduleId}')">+</button>
                    <span>${name}</span>
                    <button class="btn btn-primary" id="wishlist-btn" onclick="ShowWishList('${place.geometry.location.lat()}', '${place.geometry.location.lng()}', '${place.place_id}','${place.name}')" data-bs-toggle="modal" data-bs-target="#PopWishList"><i class="fas fa-star"></i></button>
                </div>`;
    content += '</div>';
    return content;
}

function getStarRating(rating) {
    const fullStars = Math.floor(rating);
    const halfStar = rating % 1 >= 0.5 ? 1 : 0;
    const emptyStars = 5 - fullStars - halfStar;
    let starsHtml = '<div class="star-rating">';
    for (let i = 0; i < fullStars; i++) {
        starsHtml += '<span class="full"></span>';
    }
    if (halfStar) {
        starsHtml += '<span class="half"></span>';
    }
    for (let i = 0; i < emptyStars; i++) {
        starsHtml += '<span class="empty"></span>';
    }
    starsHtml += '</div>';
    return starsHtml;
}
//#endregion
async function addPlaceToSchedule(lat, lng, placeId, scheduleId) {
    alert(`添加到行程: Lat ${lat}, Lng ${lng}, Place ID ${placeId}, Schedule ID ${scheduleId}`);
    
}

//#region 產生行程列表 generateTabs(data)
function generateTabs(data, scheduleDateIdInfo) {
    var tabsContainer = document.getElementById("tabs-container");
    tabsContainer.innerHTML = '';

    var tabContents = document.getElementById("tab-contents");
    tabContents.innerHTML = ''; // 清除之前的內容

    var isFirst = true;

    // 如果 scheduleDateIdInfo 是物件
    if (typeof scheduleDateIdInfo === 'object' && scheduleDateIdInfo !== null) {
        Object.keys(scheduleDateIdInfo).forEach((key) => { // 使用 key 作為唯一值
            const dateStr = scheduleDateIdInfo[key];           
            const dateObj = new Date(dateStr);
            if (isNaN(dateObj.getTime())) {
                console.error(`Invalid date: ${dateStr} for key ${key}`);
                return;
            }

            const formattedDate = `${dateObj.getMonth() + 1}/${dateObj.getDate()}`;

            var tabLabel = document.createElement('div');
            tabLabel.setAttribute('class', 'tab-label');
            tabLabel.setAttribute('data-target', `tab${key}-content`);
            tabLabel.setAttribute('data-schdule_day_id', key);
            tabLabel.textContent = formattedDate; // 將標籤文字設定為 m/d 格式

            if (isFirst) {
                tabLabel.classList.add('active');
                isFirst = false;
            }

            tabsContainer.appendChild(tabLabel);

            // 生成對應的內容區塊
            const tabContent = document.createElement('div');
            tabContent.setAttribute('class', `tab-content ${isFirst ? 'active' : ''}`);
            tabContent.setAttribute('id', `tab${key}-content`); // 使用 key 作為 id 的一部分
            tabContent.setAttribute('data-schdule_day_id', key);
            tabContent.setAttribute('data-placeId', data.placeId);

            tabContent.innerHTML = `
                <div class="content-item">
                    <div class="content-item-header">
                        <span class="content-item-number">${key}</span>
                        <img src="${data.picture || '/default-image.jpg'}" alt="${data.name || 'default'}" class="content-item-image">
                    </div>
                    <div class="content-item-body">
                        <div class="content-item-time">
                            <span class="icon">&#128652;</span>
                            <span class="time">${data.startTime}</span> (自訂)
                        </div>
                        <div class="content-item-location">
                            ${data.name}
                        </div>
                        <div class="content-item-detail">
                            ${data.startTime} 離開
                        </div>
                    </div>
                    <div class="content-item-menu">&#8942;</div>
                </div>
            `;

            tabContents.appendChild(tabContent);
        });
    } else {
        console.error('scheduleDateIdInfo is not an array or an object, unable to iterate.');
    }
}
//#endregion

//#region initAirDatepicker()
function initAirDatepicker() {
    try {
        let today = new Date();
        let dpMin, dpMax;
        dpMin = new AirDatepicker('#el1', {
            locale: window.airdatepickerEn,
            minDate: today,
            dateFormat(date) {
                let year = date.getFullYear();
                let month = (date.getMonth() + 1).toString().padStart(2, '0');
                let day = date.getDate().toString().padStart(2, '0');
                return `${year}-${month}-${day}`;
            },
            autoClose: true,
            onSelect({ date }) {
                dpMax.update({
                    minDate: date
                });
            }
        });

        dpMax = new AirDatepicker('#el2', {
            locale: window.airdatepickerEn,
            dateFormat(date) {
                let year = date.getFullYear();
                let month = (date.getMonth() + 1).toString().padStart(2, '0');
                let day = date.getDate().toString().padStart(2, '0');
                return `${year}-${month}-${day}`;
            },
            autoClose: true,
            onSelect({ date }) {
                dpMin.update({
                    maxDate: date
                });
            }
        });
    } catch (error) {
        console.log(`datepicker error ${error}`);
    }
}
//#endregion

//#region 行程主題更新 UploadScheduleTopic(scheduleId)__2024/8/16不使用
//function UploadScheduleTopic(scheduleId) {
//    var modifiedschedulebtn = document.getElementById("modifiedschedule_btn");
//    $(modifiedschedulebtn).on('click', function () {
//        try {
//            var themeName = $('#theme-name').text();
//        var name = $('#ScheduleName').val();
//        if (name == null || name == "") {
//            Swal.fire({
//                title: "Oops!",
//                text: `${data.message}`,
//                icon: 'warning',
//                showConfirmButton: false
//            });
//        }
//        //var startTime = $('#el1').val();
//        //var endTime = $('#el2').val();
//        // 构建请求体
//        var body = {
//            "name": name,                
//        };

//        console.log('Request body:', body);



//        fetch(`${baseAddress}/api/Schedules/UpdateSchedule/${scheduleId}`, {
//            method: "PUT",
//            body: JSON.stringify(body),
//            headers: {
//                'Authorization': `Bearer ${token}`,
//                'Content-Type': 'application/json'
//            }
//        }).then(response => {
//            if (response.ok) {
//                Swal.fire({
//                    icon: "success",
//                    title: `行程主題已更新`,
//                    showConfirmButton: false,
//                    timer: 800
//                });
//            } else {
//                return response.json().then(data => {
//                    Swal.fire({
//                        title: "Oops!",
//                        text: `${data.message}`,
//                        icon: 'warning',
//                        showConfirmButton: false
//                    });
//                    // 修改这里
//                });
//            }
//        }).catch(error => {               
//            Swal.fire({
//                title: "錯誤",
//                text: "更新行程主題時發生錯誤。",
//                icon: 'error',
//                showConfirmButton: true
//            });
//        });
//        } catch (error) {
//            console.log(`uploadtitle`, error);
//            Swal.fire({
//                title: "錯誤",
//                text: `${response.message}`,
//                icon: 'error',
//                showConfirmButton: true
//            });
//        }
//         finally {
//            $('#modifiedschedule').modal('hide');
//            setTimeout(() => {
//                location.reload();
//            }, 800);
//        }        
//    });
//}
//#endregion

//#region 把地點加入願望清單 AddPointToWishList()
async function AddPointToWishList() {
    $('#addwishlist').off('click').on('click', function () {
        try {
            var wishlistSelected = $('#wishlist_content option:selected');
            var locationcategorySelected = $('#location_categories_content option:selected');
            var wishlistId = $(wishlistSelected).attr('data-wishlistid');
            var locationcategoryId = $(locationcategorySelected).attr('data-locationcategoryid');
            var lat = $(wishlistSelected).attr('data-lat');
            var lng = $(wishlistSelected).attr('data-lng');
            var place_id = $(wishlistSelected).attr('data-placeid');
            var createat = Date.now();
            var name = $(wishlistSelected).attr('data-name').toString();
            var body = {
                "wishlistId": wishlistId,
                "locationCategoryId": locationcategoryId,
                "locationLng": lng,
                "locationLat": lat,
                "googlePlaceId": place_id,
                "name": name,
                "create_at": createat
            };

            fetch(`${baseAddress}/api/Wishlist/AddtoWishlistDetail`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(body)
            }).then(response => {
                if (response.ok) {
                    Swal.fire({
                        icon: "success",
                        title: `景點已加入清單`,
                        showConfirmButton: false,
                        timer: 1000
                    }).then(() => {

                    });
                } else {
                    return response.json().then(data => {
                        Swal.fire({
                            title: "Oops!",
                            text: `${data.message}`,
                            icon: 'warning',
                            showConfirmButton: false
                        });
                    });
                }
            }).catch(error => {
                console.log('wishlist add error', error);
                Swal.fire({
                    title: "錯誤",
                    text: "添加到願望清單時發生錯誤。",
                    icon: 'error',
                    showConfirmButton: true
                });
            });
        } catch (error) {
            console.log('wishlist add error', error);
            Swal.fire({
                title: "錯誤",
                text: "添加到願望清單時發生錯誤。",
                icon: 'error',
                showConfirmButton: true
            });
        } finally {
            $('#addwishlist').modal('hide');
            $('#PopWishList').modal('hide');
        }
    });
}
//#endregion

//#region 願望清單選單 ShowWishList(lat, lng, placeId,name)
async function ShowWishList(lat, lng, placeId, name) {
    var response = await fetch(`${baseAddress}/api/Wishlist/GetAllWishlist`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }

    });

    if (response.ok) {
        var wishlistOptions = await response.json();
        var wishlistContent = document.getElementById('wishlist_content');
        var locationCategoriesContent = document.getElementById('location_categories_content');

        wishlistContent.innerHTML = ''; 
        locationCategoriesContent.innerHTML = '';

        wishlistOptions.forEach(optionData => {
            var option = document.createElement('option');
            option.textContent = optionData.name;
            option.setAttribute('data-wishlistname', optionData.name);
            option.setAttribute('data-wishlistid', optionData.id);
            option.setAttribute('data-lat', lat);
            option.setAttribute('data-lng', lng);
            option.setAttribute('data-placeid', placeId);
            option.setAttribute('data-name', name);
            option.value = optionData.id;
            wishlistContent.appendChild(option);
        });

        wishlistContent.addEventListener('change', function () {
            var selectedWishlistId = this.value;
            locationCategoriesContent.innerHTML = '';
                
            var selectedWishlist = wishlistOptions.find(wishlist => wishlist.id == selectedWishlistId);
            if (selectedWishlist && selectedWishlist.locationCategories) {
                selectedWishlist.locationCategories.forEach(categoryData => {
                    var categoryOption = document.createElement('option');
                    categoryOption.textContent = categoryData.name;
                    categoryOption.setAttribute('data-locationcategoryid', categoryData.id);
                    categoryOption.value = categoryData.id;
                    locationCategoriesContent.appendChild(categoryOption);
                });
            }
        });
        wishlistContent.dispatchEvent(new Event('change'));
    } else {
        console.error('Failed to fetch wishlist data');
    }
}
//#endregion

//#region 行程tab標籤監聽事件 ListenScheduleListEvent()
function ListenScheduleListEvent() {
    const tabs = $('#show-schedule-row .tab-label');
    const contents = $('#show-schedule-row .tab-content');

    tabs.on('click', function () {
        const target = $(this).data('target');

        contents.removeClass('active');
        $('#' + target).addClass('active');

        tabs.removeClass('active');
        $(this).addClass('active');
    });

    // Initial check for active tab
    tabs.first().click();
}
//#endregion