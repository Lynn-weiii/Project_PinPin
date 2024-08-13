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

//#region 匯入時的資料
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

            if (results.length > 0) {
                const lastResult = results[results.length - 1];

                const { name, lat, lng, placeId, picture } = lastResult;
                const parsedLat = parseFloat(lat);
                const parsedLng = parseFloat(lng);
                if (isNaN(parsedLat) || isNaN(parsedLng)) {
                    console.error('Invalid coordinates:', { lat, lng });
                    alert('Invalid location coordinates received.');
                    return;
                }
                if (document.getElementById('theme-header')) {
                    document.getElementById('theme-header').style.backgroundImage = `url('${picture}')`;
                }

                position = { lat: parsedLat, lng: parsedLng };

                if (document.getElementById('theme-name')) {
                    document.getElementById('theme-name').innerText = name;
                }
                initMap(scheduleId, name, position);
            }
        } else {
            handleResponseErrors(response);
        }
    } catch (error) {
        alert('伺服器錯誤，請稍後再試');
        console.log(error.message);
    }
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

async function initMap(scheduleId, name, position) {
    console.log('initMap:', { scheduleId, name, position });
    globalName = name;
    globalScheduleId = scheduleId;
    const { Map } = await google.maps.importLibrary("maps");
    const { AdvancedMarkerElement } = await google.maps.importLibrary("marker");
    const { Geocoder } = await google.maps.importLibrary("geocoding");

    map = new Map(document.getElementById("map"), {
        center: position,
        zoom: 15,
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
            map.setZoom(17);
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
        infowindow.close(); // 关闭之前的 InfoWindow
        const content = createInfoWindowContent({ name: '', geometry: { location: event.latLng } }, name, scheduleId);
        infowindow.setContent(content);
        infowindow.setPosition(event.latLng);
        infowindow.open(map);

        // 在创建 InfoWindow 后，调用 geocodeLatLng 获取更多信息
        geocodeLatLng(event.latLng);
    });
} function clearMarkers() {
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
                marker.placeInfo = { name, scheduleId }; // 存储 name 和 scheduleId 到标记对象
                markers.push(marker);

                google.maps.event.addListener(marker, 'click', function () {
                    console.log('Marker clicked:', { place, name, scheduleId });
                    infowindow.setContent(createInfoWindowContent(place, name, scheduleId));
                    infowindow.open(map, marker);

                    document.querySelector('#add-place-btn').addEventListener('click', function () {
                        addPlaceToSchedule(place.geometry.location.lat(), place.geometry.location.lng(), place.place_id, scheduleId);
                    });
                });
            });

            if (displayResults.length > 0) {
                map.setCenter(displayResults[0].geometry.location);
                map.setZoom(16);
            }
        } else {
            console.error('Nearby Search failed:', status);
            alert('找不到相關地點。請重新輸入。');
        }

        $('#search_input_field').val('');
    });
}
function addPlaceToScheduleHandler(event) {
    const button = event.currentTarget;
    const lat = button.getAttribute('data-lat');
    const lng = button.getAttribute('data-lng');
    const placeId = button.getAttribute('data-place-id');
    const scheduleId = button.getAttribute('data-id');
    addPlaceToSchedule(lat, lng, placeId, scheduleId);
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
                            marker.placeInfo = { name, scheduleId }; // 存储 name 和 scheduleId 到标记对象
                            markers.push(marker);

                            console.log('Creating info window with:', { place, name, scheduleId });
                            const content = createInfoWindowContent(place, name, scheduleId);
                            console.log('Info window content:', content);

                            // 设置信息窗口内容并打开
                            infowindow.setContent(content);
                            infowindow.open(map, marker);

                            // 移除旧的监听器，防止重复监听
                            document.querySelector('#add-place-btn').removeEventListener('click', addPlaceToScheduleHandler);

                            // 添加新的监听器
                            document.querySelector('#add-place-btn').addEventListener('click', addPlaceToScheduleHandler);
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
    console.log('createInfoWindowContent called with:', { scheduleId, name });
    let content = '<div class="info-window" style="width: 350px;">';

    if (place.photos && place.photos.length > 0) {
        content += `<img src="${place.photos[0].getUrl({ maxWidth: 350 })}" alt="${place.name}" style="width: 350px;height: auto; display: block; margin: 0 auto;"><br>`;
    }

    content += `<div class="details" style="margin-top: 10px;"><strong>${place.name}</strong></div>`;
    content += `<div class="rating" style="margin-top: 5px;">${place.rating ? getStarRating(place.rating) : 'N/A'} (${place.user_ratings_total || 0})</div>`;
    content += `<div class="address" style="margin-top: 5px;">${place.vicinity || place.formatted_address}</div>`;
    content += `<div class="btn-container" style="display: flex; align-items: center; margin-top: 10px;">
                    <button class="btn-primary" id="wishlist-btn" style="background-color: gold; border: none; border-radius: 50%; width: 30px; height: 30px; margin-right: 10px;">⭐</button>
                    <span style="margin-right: 20px;">加入願望</span>
                    <button class="btn-primary" id="add-place-btn" data-lat="${place.geometry.location.lat()}" data-lng="${place.geometry.location.lng()}" data-place-id="${place.place_id}" data-id="${scheduleId}" style="background-color: green; border: none; border-radius: 50%; width: 30px; height: 30px; margin-right: 10px;">+</button>
                    <span>加入行程</span>
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
async function addPlaceToSchedule(lat, lng, placeId, scheduleId) {
    alert(`添加到行程: Lat ${lat}, Lng ${lng}, Place ID ${placeId}, Schedule ID ${scheduleId}`);
}

//#endregion
