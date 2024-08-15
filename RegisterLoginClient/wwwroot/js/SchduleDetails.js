

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

                const { name, lat, lng, placeId, picture, startTime, endTime, caneditdetail, canedittitle } = lastResult;
                const parsedLat = parseFloat(lat);
                const parsedLng = parseFloat(lng);
                console.log(name, lat, lng, placeId, picture, startTime, endTime, caneditdetail, canedittitle);
                if (isNaN(parsedLat) || isNaN(parsedLng)) {
                    console.error('Invalid coordinates:', { lat, lng });
                    alert('Invalid location coordinates received.');
                    return;
                }
                generateTabs(name, lat, lng, placeId, picture, startTime, endTime, caneditdetail, canedittitle);
                // 设置背景图片
                if (document.getElementById('theme-header')) {
                    document.getElementById('theme-header').style.backgroundImage = `url('${picture}')`;
                }

                // 定义位置对象
                const position = { lat: parsedLat, lng: parsedLng };


                if (document.getElementById('theme-name')) {
                    document.getElementById('theme-name').innerText = name;
                }
                initMap(scheduleId, name, position);
                if (canedittitle == true) {
                    $('#schtitle_edit_btn').show();
                }
                else {
                    console.log('canedittitle is false')
                }
                
                
            } else {
                handleResponseErrors(response);
            }
        }
    } catch (error) {
        /*alert('伺服器錯誤，請稍後再試');*/
        console.log(`LoadScheduleInfo error: ${error.message}`);
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

async function AddPointcreatedWishlist(lat, lng, placeId, name) {
    alert(`新增愛心點: Lat ${lat}, Lng ${lng}, Place ID ${placeId}`)
}

async function ShowWishList(lat, lng, placeId,name) {
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

//#region 行程列表

    function generateTabs(name, lat, lng, placeId, picture, startTime, endTime, caneditdetail, canedittitle) {

        var start = new Date(startTime);
        var end = new Date(endTime);
        var divamount = (end - start) / (1000 * 60 * 60 * 24) + 1;
        console.log(`generateTabs:${start}/${end}/${divamount}`);
        
                 

            
     
    }
    //#endregion



