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
            }
        });

        if (!response.ok) {
            throw new Error(`Failed to fetch schedule info: ${response.statusText}`);
        }

        const results = await response.json();
        const scheduleDateIdInfo = results.sceduleDateIdInfo;
        const scheduleDetail = results.scheduleDetail;
        if (!scheduleDetail) {
            throw new Error('scheduleDetail is undefined or invalid');
        }

        const detail = Array.isArray(scheduleDetail) ? scheduleDetail[0] : scheduleDetail;

        const {
            name,
            lat,
            lng,
            placeId,
            caneditdetail,
            canedittitle,
            startTime,
            endTime,
        } = detail;

        const parsedLat = parseFloat(lat);
        const parsedLng = parseFloat(lng);

        if (isNaN(parsedLat) || isNaN(parsedLng)) {
            throw new Error('Invalid coordinates received.');
        }

        const picture = await fetchPlacePhotoUrl(placeId);

        const data = {
            name,
            lat: parsedLat,
            lng: parsedLng,
            placeId,
            caneditdetail,
            canedittitle,
            startTime,
            endTime,
            picture,
            scheduleId,
        };
        updateUIWithScheduleInfo(data);
        
        const response2 = await fetch(`${baseAddress}/api/ScheduleDetails/${scheduleId}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
            }
        });

        if (!response2.ok) {
            throw new Error(`Failed to fetch schedule details: ${response2.statusText}`);
        }
        const data2= [];
        const textResponse = await response2.text(); 
        if (textResponse) {
            const jsonResponse = JSON.parse(textResponse);

            if (jsonResponse && jsonResponse.scheduleDetails) {
                const { scheduleDetails } = jsonResponse;

                for (let scheduleDayId of Object.keys(scheduleDetails)) {
                    const scheduleItems = scheduleDetails[scheduleDayId];
                    for (let item of scheduleItems) {
                        const placeId = item.location;
                        const pictureUrl = await fetchPlacePhotoUrl(placeId);

                        const transportation = item.transportations.length > 0 ? item.transportations[0] : null;

                        data2.push({
                            sort: item.sort,  // 使用冒号而非等号
                            id: item.id,
                            scheduleDayId: item.scheduleDayId,
                            userId: item.userId,
                            locationName: item.locationName,
                            placeId: placeId,
                            startTime: item.startTime,
                            endTime: item.endTime,
                            lat: item.lat,
                            lng: item.lng,
                            pictureUrl: pictureUrl,
                            transportation: transportation || null
                        });
                    }
                }

                // 对 data2 进行排序
                data2.sort((a, b) => a.sort - b.sort);
            } else {
                console.log('No schedule details found or scheduleDetails is missing.');
            }
        } else {
            console.log('Empty response, no data to process.');
        }        
        generateDateList(scheduleDateIdInfo);
        generateTabLabel(scheduleDateIdInfo);
        generateTabContents(data2, scheduleDateIdInfo); 
        
            document.getElementById('tab-contents').addEventListener('click', function (event) {
                if (event.target.classList.contains('delete-btn')) {
                    const Id = event.target.getAttribute('data-id');
                    const scheduleDayId = event.target.getAttribute('data-scheduleDayId');
                    const scheduleId = event.target.getAttribute('data-scheduleId');
                    
                    DeleteSchedule(Id, scheduleDayId, scheduleId);
                }
            });
        console.log('data2:', data2);
    } catch (error) {
        console.error('Error fetching schedule info:', error);
    }
}
function generateDefaultContent(scheduleDateIdInfo) {
    const data2 = []; 
    generateDateList(scheduleDateIdInfo);
    generateTabLabel(scheduleDateIdInfo);
    generateTabContents(data2, scheduleDateIdInfo); // 传递空的 data2
}
function updateUIWithScheduleInfo(data) {
    if (document.getElementById('theme-header')) {
        document.getElementById('theme-header').style.backgroundImage = `url('${data.picture}')`;
    }

    const travelduring = document.getElementById('travelduring');
    if (travelduring) {
        travelduring.innerHTML = `${data.startTime} <i class="fa-solid fa-arrow-right"></i> ${data.endTime}`;
    }

    if (document.getElementById('theme-name')) {
        document.getElementById('theme-name').innerText = data.name;
    }

    const position = { lat: data.lat, lng: data.lng };
    initMap(data.scheduleId, data.name, position, data.placeId);

    if (data.canedittitle) {
        
    } else {
        console.log('canedittitle is false');
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
                resolve('/images/NoImg.png'); // 没有照片时返回 null
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

//#region google Map API 
let map, marker, autocomplete, infowindow, position, service, keyword, searchTimeout, idleListener, globalName, globalScheduleId, polylines, directionsService,directionsRenderer;
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
        zoom: 16,
        mapId: "d4432686758d8acc",
    });
    
    infowindow = new google.maps.InfoWindow({
        maxWidth: 640,
        maxHeight: 460
    });
    geocoder = new Geocoder();
    directionsService = new google.maps.DirectionsService();
    directionsRenderer = new google.maps.DirectionsRenderer({
        map: map,
    });
    service = new google.maps.places.PlacesService(map);
    var drawingManager = new google.maps.drawing.DrawingManager();
    drawingManager.setMap(map);
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
            map.setZoom(16);
        }

        const marker = new AdvancedMarkerElement({
            position: place.geometry.location,
            map: map,
            title: place.name,
        });
        marker.placeInfo = { name, scheduleId };
        markers.push(marker);

        console.log('Autocomplete place selected:', { place, name, scheduleId });
        const content = createInfoWindowContent(place, name, scheduleId);
        infowindow.setContent(content);
        infowindow.open(map, marker);

        
        document.querySelector('#add-place-btn').removeEventListener('click');        
        document.querySelector('#add-place-btn').addEventListener('click');

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

    map.addListener('click', function (event) {
        console.log('Map clicked:', { lat: event.latLng.lat(), lng: event.latLng.lng(), name, scheduleId });
        infowindow.close(); 
        const content = createInfoWindowContent({ name: '', geometry: { location: event.latLng } }, name, scheduleId);
        infowindow.setContent(content);
        infowindow.setPosition(event.latLng);
        infowindow.open(map);
        geocodeLatLng(event.latLng);
    });
}
function clearMarkers() {
    markers.forEach(marker => marker.setMap(null));
    markers = [];
    directionsRenderer.set('directions', null);
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
                            console.log('Place details:', { place, name, scheduleId });

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
//#endregion

//#region 景點詳細資訊 createInfoWindowContent(place, name, scheduleId)
function createInfoWindowContent(place, name, scheduleId) {
    const imageUrl = place.photos && place.photos.length > 0
        ? place.photos[0].getUrl({ maxWidth: 290, maxHeight: 290 })
        : ''; // 这里加上默认值（比如空字符串）

    const starsHtml = getStarRating(place.rating); // 获取 starsHtml

    let content = `
    <div class="info-window" style="max-width: 640px; max-height: 460px; display: flex; padding: 15px; font-family: Arial, sans-serif; box-sizing: border-box;" id="info-window">
        <!-- 照片部分 -->
        <div class="location-image" style="flex: 0 0 50%; max-width: 55%; padding: 5px; box-sizing: border-box;">
            <img src="${imageUrl}" alt="${place.name}" class="info-window-photo" style="border-radius: 15px; width: 100%; height: 100%; object-fit: cover;">
        </div>
        <!-- 详细信息部分 -->
        <div class="location-details mb-3" style="flex: 0 0 50%; max-width: 45%; padding-left: 10px; box-sizing: border-box; overflow-y: auto;">
            <h2 style="margin-top: 0; font-size: 20px; color: #333; overflow-wrap: break-word;">${place.name}</h2>
            <div style="margin: 5px 0; font-size: 16px; color: #FF9900;">
                Google 評價:(${place.rating || '沒有評分'}) ${starsHtml} ${place.user_ratings_total ? `(${place.user_ratings_total} 則評價)` : ''}
            </div>
            <p style="margin: 5px 0; font-size: 14px; color: #666; overflow-wrap: break-word;">${place.vicinity || place.formatted_address}</p>

            <div class="contact" style="margin: 10px 0; font-size: 14px; overflow-wrap: break-word;">
                <i class="fas fa-phone-alt" style="margin-right: 5px; color: #666;"></i>
                <span>${place.formatted_phone_number || 'N/A'}</span>
            </div>

            <div class="mt-2 mb-2">
                <p style="font-size: 14px; color: #333;"><strong>營業時間:</strong></p>
                <ul style="list-style-type: none; padding-left: 0; font-size: 14px; color: #666; max-height: 100px; overflow-y: auto;">`;

    if (place.current_opening_hours && place.current_opening_hours.weekday_text) {
        for (let i = 0; i < place.current_opening_hours.weekday_text.length; i++) {
            content += `<li>${place.current_opening_hours.weekday_text[i]}</li>`;
        }
    } else {
        content += `<li>無營業時間資訊</li>`;
    }

    content += `</ul>
            </div>
            <div class="btn-container mb-1" style="font-size:14px;margin-top: 5px;display:flex;position:absolute;bottom: 10px;right: 10px;">
                <button class="btn btn-primary btn-sm" id="add-place-btn" data-id="${scheduleId}" onclick="addscheduledate('${place.geometry.location.lat()}', '${place.geometry.location.lng()}', '${place.place_id}', '${place.name}')">+<strong style="margin-left: 10px;">加入行程</strong></button>
                <button class="btn btn-primary mx-3 btn-sm" id="wishlist-btn" onclick="ShowWishList('${place.geometry.location.lat()}', '${place.geometry.location.lng()}', '${place.place_id}','${place.name}')" data-bs-toggle="modal" data-bs-target="#PopWishList">
                    <i class="fas fa-star"></i><strong style="margin-left: 10px;">加入願望清單</strong>
                </button>
            </div>
        </div>
    </div>`;

    return content;
}

function getStarRating(rating) {
    const fullStars = rating ? Math.floor(rating) : 0;
    const halfStar = rating && rating % 1 >= 0.5 ? 1 : 0;
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

//#region 新增景點到日程
function addscheduledate(lat, lng, placeId, name) {
    $('#AddPointWhichDayList').modal('show');

    // 先移除之前可能已存在的点击事件监听器
    $('#addPointWhichDayList').off('click').on('click', async function () {
        var daylistSelected = $('#date-list option:selected');
        var daylistSelectedId = daylistSelected.attr('data-schdule_day_id');
        console.log(`dailyID: ${daylistSelectedId}`);
        var TransportationCategoryId = $('#transportation-list option:selected').val();
        var stratTime = $('#start-time').val();
        var endTime = $('#end-time').val();
        stratTime += ":00";
        endTime += ":00";
        var day = daylistSelected.text();
        console.log(`transportationId: ${TransportationCategoryId}`);
        var scheduleId = sessionStorage.getItem('scheduleId');

        var body = {
            "ScheduleDayId": daylistSelectedId,
            "LocationName": name,
            "Location": placeId,
            "StartTime": stratTime,
            "EndTime": endTime,
            "lat": lat,
            "lng": lng,
            "TransportationCategoryId": TransportationCategoryId
        };
        console.log(JSON.stringify(body));

        try {
            let response = await fetch(`${baseAddress}/api/ScheduleDetails`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(body)
            });

            if (response.ok) {
                Swal.fire({
                    title: "成功",
                    text: `已新增 ${name} 到 ${day} 的行程中！`,
                    icon: "success",
                    showConfirmButton: false,
                    timer: 1500
                }).then(() => {
                    console.log(`go refresh add schedule id :${scheduleId}`);
                    refreshlist(scheduleId);
                });
            } else if (response.status === 409) {
                let data = await response.json();
                Swal.fire({
                    title: "Are you sure?",
                    text: data.message,
                    icon: "warning",
                    iconColor: '#E4003A',
                    showCancelButton: true,
                    confirmButtonColor: "#405D72",
                    cancelButtonColor: "#F9E0BB",
                    confirmButtonText: "確認再新增"
                }).then(async (result) => {
                    if (result.isConfirmed) {
                        let response2 = await fetch(`${baseAddress}/api/ScheduleDetails/override-schedule-detail`, {
                            method: 'POST',
                            headers: {
                                'Authorization': `Bearer ${token}`,
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify(body)
                        });
                        if (response2.ok) {
                            Swal.fire({
                                title: "成功",
                                text: `已新增 ${name} 到 ${day} 的行程中！`,
                                icon: "success",
                                showConfirmButton: false,
                                timer: 1500
                            }).then(() => {
                                console.log(`go refresh add schedule id :${scheduleId}`);
                                refreshlist(scheduleId);
                            });
                        } else {
                            Swal.fire({
                                title: "Oops!",
                                text: "新增失敗，請重新嘗試。",
                                icon: "error",
                                showConfirmButton: false,
                                timer: 1500
                            });
                        }
                    }
                });
            } else {
                await handleResponseErrors(response);
                Swal.fire({
                    title: "Oops!",
                    text: "新增失敗，請重新嘗試。",
                    icon: "error",
                    showConfirmButton: false,
                    timer: 1500
                });
            }
        } catch (error) {
            console.error('Error adding place to schedule:', error);
            Swal.fire({
                title: "Oops!",
                text: "新增失敗，請重新嘗試。",
                icon: "error",
                showConfirmButton: false,
                timer: 1500
            });
        } finally {
            $('#AddPointWhichDayList').modal('hide');
            $('#search_input_field').val('');
            clearMarkers();
        }
    });
}
//#endregion

//#region 產生日期列表 generateDateList(scheduleDateIdInfo)
    function generateDateList(scheduleDateIdInfo) {
        var dateList = document.getElementById("date-list");
        dateList.innerHTML = '';
        var scheduleId = sessionStorage.getItem("scheduleId");
        console.log(`generateDateList get scheduleId: ${scheduleId}`);
        if (typeof scheduleDateIdInfo === 'object' && scheduleDateIdInfo !== null) {
            Object.keys(scheduleDateIdInfo).forEach((key) => {
                const dateStr = scheduleDateIdInfo[key];
                const dateObj = new Date(dateStr);
                if (isNaN(dateObj.getTime())) {
                    console.error(`Invalid date: ${dateStr} for key ${key}`);
                    return;
                }

                const formattedDate = `${dateObj.getFullYear()}/${dateObj.getMonth() + 1}/${dateObj.getDate()}`;
            
                const dateItem = document.createElement('option');
                dateItem.setAttribute('class', 'date-item');
                dateItem.setAttribute('data-scheduleId', `${scheduleId }`);
                dateItem.setAttribute('data-schdule_day_id', key);
                dateItem.textContent = formattedDate; 

                dateList.appendChild(dateItem);
            });
        } else {
            console.error('scheduleDateIdInfo is not an array, unable to iterate.');
        }
    }
//#endregion

//#region 產生行程列表 generateTabs(data)
async function generateTabLabel(scheduleDateIdInfo) {
    var tablabels = document.getElementById("tabs-label");
    tablabels.innerHTML = ''; // 清除之前的内容

    if (typeof scheduleDateIdInfo === 'object' && scheduleDateIdInfo !== null) {
        let isFirst = true;

        for (const key of Object.keys(scheduleDateIdInfo)) {
            const dateStr = scheduleDateIdInfo[key];
            const dateObj = new Date(dateStr);
            if (isNaN(dateObj.getTime())) {
                console.error(`Invalid date: ${dateStr} for key ${key}`);
                continue;
            }

            const formattedDate = `${dateObj.getMonth() + 1}/${dateObj.getDate()}`;
            const tabLabel = document.createElement('button');
            tabLabel.setAttribute('class', `tab-button ${isFirst ? 'active' : ''}`);
            tabLabel.setAttribute('data-id', `${key}`);
            tabLabel.setAttribute('data-schdule_day_id', key);
            tabLabel.setAttribute('data-target', `tab-${key}`);
            tabLabel.textContent = formattedDate;

            tablabels.appendChild(tabLabel);

            
            isFirst = false;
        }
    } else {
        console.error('scheduleDateIdInfo is not an array or an object, unable to iterate.');
    }
}
async function generateTabContents(data2, scheduleDateIdInfo) {
    var tabContents = document.getElementById("tab-contents");
    tabContents.innerHTML = '';

    if (typeof scheduleDateIdInfo === 'object' && scheduleDateIdInfo !== null) {
        let isFirst = true;

        for (const key of Object.keys(scheduleDateIdInfo)) {
            const tabContent = document.createElement('div');
            tabContent.setAttribute('class', `content ${isFirst ? 'active' : ''}`);
            tabContent.setAttribute('id', `${key}`);
            tabContent.setAttribute('data-id', `tab-${key}`);
            tabContent.setAttribute('style', `background: transparent;`);
            tabContent.setAttribute('data-schdule_day_id', key);

            let hasContent = false;
            const placeArray = Object.values(data2);
            let itemIndex = 1;
            let previousLatLng = null;
            let firstLatLng = null;

            for (const place of placeArray) {
                if (place.scheduleDayId && place.scheduleDayId === parseInt(key)) {
                    hasContent = true;

                    const transportationMode = place.transportation ? getTransportationMode(place.transportation.transportationCategoryId) : '❓';
                    const travelMode = getTravelMode(place.transportation?.transportationCategoryId);

                    if (!firstLatLng) {
                        firstLatLng = { lat: place.lat, lng: place.lng };
                    }

                    const contentItem = document.createElement('div');
                    contentItem.classList.add('content-item');
                    contentItem.setAttribute('data-placeId', `${place.placeId}`);
                    contentItem.setAttribute('data-sort', `${place.sort}`);
                    contentItem.setAttribute('data-lat', `${place.lat}`);
                    contentItem.setAttribute('data-lng', `${place.lng}`);
                    contentItem.innerHTML = `
                        <div class="content-item-header">
                            <span class="content-item-number">${itemIndex}</span>
                            <img src="${place.pictureUrl || '~/images/NoImg.png'}" alt="${place.locationName || 'default'}" class="content-item-image">
                        </div>
                        <div class="content-item-body">
                            <div class="content-item-time">
                                <span class="icon">${transportationMode}</span>
                                <span class="time">${place.startTime}</span>
                            </div>
                            <div class="content-item-location">
                                ${place.locationName}
                            </div>
                            <div class="content-item-detail">
                                ${place.startTime} 離開
                            </div>
                            <div class="button-group">
                                <button class="delete-btn"  data-scheduleDayId="${place.scheduleDayId}" data-id="${place.id}" >
                                    刪除景點
                                </button>
                            </div>
                        </div>
                    `;

                    if (previousLatLng) {
                        const routeInfo = await getRouteInfo(previousLatLng, { lat: place.lat, lng: place.lng }, travelMode);
                        if (routeInfo) {
                            const routeDiv = document.createElement('div');
                            routeDiv.classList.add('route-info');
                            routeDiv.innerHTML = `
                                <div class="route-description">
                                    <span class="route-icon">${getTravelModeIcon(routeInfo.travelMode)}</span>
                                    <span>${routeInfo.durationText}</span> (${routeInfo.distanceText})
                                </div>
                            `;
                            tabContent.appendChild(routeDiv);
                        }
                    }

                    previousLatLng = { lat: place.lat, lng: place.lng };

                    tabContent.appendChild(contentItem);
                    itemIndex++;
                }
            }

            if (!hasContent) {
                const noContentMessage = document.createElement('div');
                noContentMessage.setAttribute('id', `tab${key}`);
                noContentMessage.classList.add('no-content-message');
                noContentMessage.textContent = '目前沒有安排的景點唷~';
                tabContent.appendChild(noContentMessage);
            }

            tabContents.appendChild(tabContent);

            if (isFirst) {
                isFirst = false;
            }
        }
    } else {
        console.error('scheduleDateIdInfo不是array也不是object。');
    }
}
async function getRouteInfo(origin, destination) {

    const travelModes = [
        google.maps.TravelMode.TRANSIT,
        google.maps.TravelMode.DRIVING,
        google.maps.TravelMode.WALKING
    ];

    for (const mode of travelModes) {
        try {
            const directionsService = new google.maps.DirectionsService();
            const request = {
                origin: new google.maps.LatLng(origin.lat, origin.lng),
                destination: new google.maps.LatLng(destination.lat, destination.lng),
                travelMode: mode,
            };

            const result = await new Promise((resolve, reject) => {
                directionsService.route(request, (response, status) => {
                    if (status === google.maps.DirectionsStatus.OK) {
                        resolve(response.routes[0].legs[0]);
                    } else {
                        reject(status);
                    }
                });
            });

            return {
                travelMode: mode,
                durationText: result.duration.text,
                distanceText: result.distance.text
            };

        } catch (status) {
            console.warn(`無法取得 ${mode} 的路線，原因：${status}`);
            if (status === google.maps.DirectionsStatus.ZERO_RESULTS) {
                continue;
            } else {
                throw new Error(`無法取得路線，原因：${status}`);
            }
        }
    }

    throw new Error('查無路線。');
}
function getTransportationMode(categoryId) {
    switch (categoryId) {
        case 1:
            return '🚗'; // 開車
        case 3:
            return '🚌'; // 大眾運輸
        case 4:
            return '🚴'; // 腳踏車
        case 5:
            return '🚶'; // 步行
        default:
            return '❓'; // 未知或其他類型
    }
}
function getTravelMode(categoryId) {
    switch (categoryId) {
        case 1:
            return google.maps.TravelMode.DRIVING; // 開車
        case 3:
            return google.maps.TravelMode.TRANSIT; // 大眾運輸
        case 4:
            return google.maps.TravelMode.BICYCLING; // 腳踏車
        case 5:
            return google.maps.TravelMode.WALKING; // 步行
        default:
            return google.maps.TravelMode.DRIVING; // 默認為開車
    }
}
function getTravelModeIcon(travelMode) {
    switch (travelMode) {
        case google.maps.TravelMode.DRIVING:
            return '🚗'; // 開車
        case google.maps.TravelMode.BICYCLING:
            return '🚴'; // 腳踏車
        case google.maps.TravelMode.TRANSIT:
            return '🚌'; // 大眾運輸
        case google.maps.TravelMode.WALKING:
            return '🚶'; // 步行
        default:
            return '❓'; // 未知或其他類型
    }
}
function initMarkers(markersData) {
    clearMarkers(); 

    markersData.forEach(markerData => {
        const marker = new google.maps.Marker({
            position: { lat: markerData.lat, lng: markerData.lng },
            map: map,
            title: markerData.title
        });

        markers.push(marker); 
    });

    
    if (markersData.length > 0) {
        map.setCenter({ lat: markersData[0].lat, lng: markersData[0].lng });
        map.setZoom(15); // 根据需要调整缩放级别
    }
}
function calculateAndDisplayRoute(locations) {
    if (locations.length < 2) {
        return;
    }

    const waypoints = locations.slice(1, locations.length - 1).map(location => ({
        location: new google.maps.LatLng(location.lat, location.lng),
        stopover: true,
    }));

    directionsService.route({
        origin: new google.maps.LatLng(locations[0].lat, locations[0].lng),
        destination: new google.maps.LatLng(locations[locations.length - 1].lat, locations[locations.length - 1].lng),
        waypoints: waypoints,
        travelMode: google.maps.TravelMode.DRIVING, // 可以选择不同的出行方式，如 'DRIVING', 'WALKING', 'BICYCLING', 'TRANSIT'
    }, (response, status) => {
        if (status === 'OK') {
            directionsRenderer.setDirections(response);

            // 获取交通方式和时间信息
            const route = response.routes[0];
            let totalDuration = 0;

            route.legs.forEach((leg, index) => {
                totalDuration += leg.duration.value; // 总时间（以秒为单位）
                const travelMode = leg.steps[0].travel_mode; // 获取当前路段的交通方式

                // 在页面或地图上显示交通方式和时间信息
                console.log(`Leg ${index + 1}: ${travelMode}, Duration: ${leg.duration.text}`);

                const travelInfo = document.createElement('div');
                travelInfo.innerHTML = `Leg ${index + 1}: ${travelMode}, Duration: ${leg.duration.text}`;
                document.getElementById('route-info').appendChild(travelInfo);
            });

            // 总时间以分钟为单位显示
            const totalMinutes = Math.round(totalDuration / 60);
            console.log(`Total duration: ${totalMinutes} minutes`);

            const totalInfo = document.createElement('div');
            totalInfo.innerHTML = `Total duration: ${totalMinutes} minutes`;
            document.getElementById('route-info').appendChild(totalInfo);

        } else {
            window.alert('Directions request failed due to ' + status);
        }
    });
}
function tabsscoller() {
    const tabs = document.querySelector(".wrapper");

    tabs.onclick = e => {
        const clickedElement = e.target.closest(".tab-button");
        if (clickedElement && clickedElement.classList.contains('tab-button') && clickedElement.dataset.id) {
            const id = clickedElement.dataset.id;
            const tabButtons = document.querySelectorAll(".tab-button");
            const contents = document.querySelectorAll(".content");

            tabButtons.forEach(btn => btn.classList.remove("active"));
            clickedElement.classList.add("active");

            contents.forEach(content => content.classList.remove("active"));
            clearMarkers();

            const element = document.getElementById(id);
            if (element) {
                element.classList.add("active");

                const contentItems = element.querySelectorAll('.content-item');
                let locations = [];

                contentItems.forEach(item => {
                    const lat = parseFloat(item.getAttribute('data-lat'));
                    const lng = parseFloat(item.getAttribute('data-lng'));

                    if (!isNaN(lat) && !isNaN(lng)) {
                        locations.push({ lat: lat, lng: lng });
                    } else {
                        console.error("Invalid lat/lng for content item:", item);
                    }
                });

                if (locations.length > 0) {
                    initMarkers(locations);
                    calculateAndDisplayRoute(locations); 
                }
            } else {
                console.error("Content element not found for ID:", id);
            }
        } else {
            console.error("Clicked element is not a valid tab-button or doesn't have a data-id");
        }
    };
}

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
                        timer: 1500
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
            infowindow.close();
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

//#region 安裝選擇時間picker initAirDatepicker()
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

//#region 刪除行程
async function DeleteSchedule(id, scheduleDayId, scheduleId) {
    try {

        var response = await fetch(`${baseAddress}/api/ScheduleDetails/${id}/${scheduleDayId}`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
            },
        });

        if (response.ok) {
            console.log('删除成功');
            swal.fire({
                icon: "success",
                title: `行程已刪除`,
                showConfirmButton: false,
                timer: 1500
            }).then(() => {               
                refreshlist();
            });
            
        } else {
            console.error('删除失败');
            swal.fire({
                title: "Oops!",
                text: `刪除行程失敗`,
                icon: 'warning',
                showConfirmButton: false
            });
        }
    } catch (error) {
        console.error('删除操作中发生错误:', error); swal.
            fire({
                title: "錯誤",
                text: "刪除行程時發生錯誤。",
                icon: 'error',
                showConfirmButton: true
            });
    } 
}
//#endregion

//#region 更新行程列表
async function refreshlist() {
    try {
        var scheduleId = sessionStorage.getItem("scheduleId");
        console.log(`refreshlist get scheduleid:${scheduleId}`);

        const response3 = await fetch(`${baseAddress}/api/Schedules/Entereditdetailsch/${scheduleId}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
            }
        });

        if (!response3.ok) {
            throw new Error(`Failed to fetch schedule info: ${response3.statusText}`);
        }

        const results3 = await response3.json();
        const scheduleDateIdInfo = results3.sceduleDateIdInfo;
        const scheduleDetail = results3.scheduleDetail;
        if (!scheduleDetail) {
            throw new Error('scheduleDetail is undefined or invalid');
        }

        const response2 = await fetch(`${baseAddress}/api/ScheduleDetails/${scheduleId}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
            }
        });

        if (!response2.ok) {
            throw new Error(`Failed to fetch schedule details: ${response2.statusText}`);
        }

        const textResponse = await response2.text();
        let data2 = [];
        if (textResponse) {
            const jsonResponse = JSON.parse(textResponse);
            if (jsonResponse && jsonResponse.scheduleDetails) {
                const { scheduleDetails } = jsonResponse;

                for (let scheduleDayId of Object.keys(scheduleDetails)) {
                    const scheduleItems = scheduleDetails[scheduleDayId];
                    for (let item of scheduleItems) {
                        const placeId = item.location;
                        let pictureUrl = '';

                        try {
                            pictureUrl = await fetchPlacePhotoUrl(placeId);
                        } catch (error) {
                            console.error(`Failed to fetch picture URL for place ID ${placeId}: ${error}`);
                        }

                        const transportation = item.transportations.length > 0 ? item.transportations[0] : null;

                        data2.push({
                            sort: item.sort,
                            id: item.id,
                            scheduleDayId: item.scheduleDayId,
                            userId: item.userId,
                            locationName: item.locationName,
                            placeId: placeId,
                            startTime: item.startTime,
                            endTime: item.endTime,
                            lat: item.lat,
                            lng: item.lng,
                            pictureUrl: pictureUrl,
                            transportation: transportation || null
                        });
                    }
                }

                // 对 data2 进行排序
                data2.sort((a, b) => a.sort - b.sort);
            } else {
                console.log('No schedule details found or scheduleDetails is missing.');
            }
        } else {
            console.log('Empty response, no data to process.');
        }

        generateDateList(scheduleDateIdInfo);
        generateTabLabel(scheduleDateIdInfo);
        generateTabContents(data2, scheduleDateIdInfo);

    } catch (error) {
        console.error(`Error in refreshlist: ${error}`);
        Swal.fire({
            title: "Oops!",
            text: "Failed to refresh the list. Please try again.",
            icon: "error",
            showConfirmButton: false,
            timer: 1500
        });
    }



    document.getElementById('tab-contents').addEventListener('click', function (event) {
        if (event.target.classList.contains('delete-btn')) {
            const Id = event.target.getAttribute('data-id');
            const scheduleDayId = event.target.getAttribute('data-scheduleDayId');
            const scheduleId = event.target.getAttribute('data-scheduleId');

            DeleteSchedule(Id, scheduleDayId, scheduleId);
        }
    });
    console.log('data2:', data2);
}
//#endregion

//#region 天氣資訊 可以呼叫但是div要自己刻，如果用官方的不能用後台呼叫的資料渲染
//async function GetWeatherInfo(lat, lng) {
//    try {
//        const url=`${baseAddress}/api/Weather?lat=${lat}&lon=${lng}&units=metric`;
//        var response = await fetch(url, {
//            method: 'GET',
//            headers: {
//                'Authorization': `Bearer ${token}`,
//                'Content-Type': 'application/json'
//            },
//        });

//        if (response.ok) {
//            const weatherData = await response.json();
//            const container = document.getElementById('weather-container');
//            container.innerHTML = '';
//            weatherData.forEach(dayData => {
//                dayData.Day.forEach(weather => {
//                    const timeOfDay = weather.IsMorning ? '早上' : '下午/晚上';
//                    const date = new Date(weather.DateTime).toLocaleDateString('zh-TW', { month: 'long', day: 'numeric' });

//                    const weatherDiv = document.createElement('div');
//                    weatherDiv.style.display = 'flex';
//                    weatherDiv.style.flexDirection = 'column';
//                    weatherDiv.style.padding = '10px';
//                    weatherDiv.style.borderRadius = '10px';
//                    weatherDiv.style.backgroundColor = '#ffffff';
//                    weatherDiv.style.boxShadow = '0px 2px 4px rgba(0, 0, 0, 0.1)';
//                    weatherDiv.style.marginBottom = '10px';

//                    weatherDiv.innerHTML = `
//                    <div style="font-weight: bold; font-size: 16px;">${date} ${timeOfDay}</div>
//                    <div style="font-size: 14px;">溫度: ${weather.Temp.toFixed(1)}°C</div>
//                    <div style="font-size: 14px;">降雨機率: ${(weather.ChanceOfRain * 100).toFixed(1)}%</div>
//                `;
//                    container.appendChild(weatherDiv);
//                });
//            });
//        } else {
//            console.error(`GetWeatherInfo failed with status ${response.status}`);
//        }
//    } catch (error) {
//        console.log(`GetWeatherInfo error ${error}`);
//    }
//    return null;
//}

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

//#region timepicker


function calculateEndTime() {
    const startTime = document.getElementById('start-time').value;
    const durationHours = parseInt(document.getElementById('duration-hours').value) || 0;
    const durationMinutes = parseInt(document.getElementById('duration-minutes').value) || 0;

    if (startTime) {
        const [hours, minutes] = startTime.split(':').map(Number);
        let endHours = hours + durationHours;
        let endMinutes = minutes + durationMinutes;

        // 处理分钟进位
        if (endMinutes >= 60) {
            endHours += Math.floor(endMinutes / 60);
            endMinutes = endMinutes % 60;
        }

        // 处理24小时制的情况
        if (endHours >= 24) {
            endHours = endHours % 24;
        }

        // 格式化小时和分钟
        const formattedEndHours = String(endHours).padStart(2, '0');
        const formattedEndMinutes = String(endMinutes).padStart(2, '0');

        document.getElementById('end-time').value = `${formattedEndHours}:${formattedEndMinutes}`;
    }
}
