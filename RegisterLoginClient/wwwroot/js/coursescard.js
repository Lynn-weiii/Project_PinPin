
function createCourseCard(course, isGroup = false) {
    var courseItem = document.createElement('div');
    courseItem.className = 'item course_card_owl_item';
    var pictureUrl = course.pictureUrl ? course.pictureUrl : '/images/course/course_01.jpg';
    
    courseItem.innerHTML = `
    
        <div class="course_card">
            <div class="course_card_container">
                 <div class="course_card_top">
                    <div class="course_menu_section">
                     <div class="dropdown" style="display:none;">
                        <button class="btn btn-secondary dropdown-toggle" type="button" id="dropdownMenuButton1"
                            data-bs-toggle="dropdown" aria-expanded="false" style="display:none;">
                            Dropdown button
                        </button>
                        <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton1" style="display:none;">
                            <li style="display:none;"><a class="dropdown-item" href="#">Action</a></li>
                            <li style="display:none;"><a class="dropdown-item" href="#">Another action</a></li>
                            <li style="display:none;"><a class="dropdown-item" href="#">Something else here</a></li>
                        </ul>
                    </div>
                    <div class="dropdown">
                        <div class="course_card_category trans_200" type="button" id="courseDropdown-${course.id}" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="fa fa-ellipsis"></i>
                        </div>
                        <ul class="dropdown-menu" aria-labelledby="courseDropdown-${course.id}" >
                             <li><a class="dropdown-item" data-fun="MemberManager" data-id="${course.id}" data-name="${course.name}" data-bs-toggle="modal" data-bs-target="#schedule_authority_modal">成員管理</a></li>
                             <li><a class="dropdown-item" data-fun="InviteMember" data-id="${course.id}" data-name="${course.name}" data-bs-toggle="modal" data-bs-target="#schedule_authority_modal">邀請成員</a></li>
                            <li><a class="dropdown-item" data-fun="Delete" data-id="${course.id}" data-name="${course.name}">刪除行程</a></li>
                        </ul>
                    </div>
                    </div>
                    <a href="#" class="course_card_link edit_btn" data-fun="EditMainSch" data-id="${course.id}">
                    <div class="course_card_pic">
                        <img src="${pictureUrl}" style="width:430px;height:286px">
                    </div>
                    <div class="course_card_content">
                        <div class="course_card_meta d-flex flex-row align-items-center"></div>
                        <div class="course_card_title">
                            <h3>${course.name}</h3>
                        </div>
                        <div class="course_card_author">
                            <span>by ${course.userName}</span>
                        </div>
                        <div class="course_card_rating d-flex flex-row align-items-center">
                            <h5>${course.startTime}</h5>
                            <h5 style="padding:5px;"><i class="fa-solid fa-arrow-right" style="color: #0e4e3b;"></i></h5>
                            <h5>${course.endTime}</h5>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </a>
    `;
    return courseItem;
}


function createGroupCourseCard(gcourse) {
    var gcourseItem = document.createElement('div');
    gcourseItem.className = 'item course_card_owl_item';
    var pictureUrl = gcourse.pictureUrl ? gcourse.pictureUrl : '/images/course/course_01.jpg';
    
    gcourseItem.innerHTML = `
    
        <div class="course_card">
            <div class="course_card_container">
                <div class="course_card_top">
                    <div class="course_menu_section">
                     <div class="dropdown" style="display:none;">
                        <button class="btn btn-secondary dropdown-toggle" type="button" id="dropdownMenuButton1"
                            data-bs-toggle="dropdown" aria-expanded="false" style="display:none;">
                            Dropdown button
                        </button>
                        <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton1" style="display:none;">
                            <li style="display:none;"><a class="dropdown-item" href="#">Action</a></li>
                            <li style="display:none;"><a class="dropdown-item" href="#">Another action</a></li>
                            <li style="display:none;"><a class="dropdown-item" href="#">Something else here</a></li>
                        </ul>
                    </div>
                    <div class="dropdown">
                        <div class="course_card_category trans_200" type="button" id="courseDropdown-${gcourse.id}" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="fa fa-ellipsis"></i>
                        </div>
                        <ul class="dropdown-menu" aria-labelledby="courseDropdown-${gcourse.id}" >
                             <li><a class="dropdown-item" data-fun="CheckManager" data-id="${gcourse.id}" data-name="${gcourse.name}">查看成員</a></li>
                             <li><a class="dropdown-item" data-fun="Exit" data-id="${gcourse.id}" data-name="${gcourse.name}">離開</a></li>
                        </ul>
                    </div>
                    </div>
                    <a href="#" class="course_card_link edit_btn" data-fun="EditGroupSch" data-id="${gcourse.id}">
                    <div class="course_card_pic">
                        <img src="${pictureUrl}" style="width:430px;height:286px">
                    </div>
                    <div class="course_card_content">
                        <div class="course_card_meta d-flex flex-row align-items-center"></div>
                        <div class="course_card_title">
                            <h4>${gcourse.name}</h4>
                        </div>
                        <div class="course_card_author">
                            <span>by ${gcourse.userName}</span>
                        </div>
                        <div class="course_card_rating d-flex flex-row align-items-center">
                            <h5>${gcourse.startTime}</h5>
                            <h5 style="padding:5px;"><i class="fa-solid fa-arrow-right" style="color: #0e4e3b;"></i></h5>
                            <h5>${gcourse.endTime}</h5>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </a>
    `;
    return gcourseItem;
}




// 使用通用函数创建课程卡片
function renderCourses(data) {
    var container = document.getElementById('course-container');
    container.innerHTML = '';

    data.forEach(course => {
        var courseItem = createCourseCard(course);
        container.appendChild(courseItem);

        // 初始化 Bootstrap 下拉菜单
        var dropdownToggleList = [].slice.call(courseItem.querySelectorAll('[data-bs-toggle="dropdown"]'));
        dropdownToggleList.map(function (dropdownToggleEl) {
            return new bootstrap.Dropdown(dropdownToggleEl);
        });
    });
    
    // 初始化轮播等其他功能
    initPopularCoursesSlider('#course-container');
}

// 使用通用函数创建组课程卡片
function groupCourses(data2) {
    var gcontainer = document.getElementById('group_course-container');
    gcontainer.innerHTML = ''; // 清空现有内容

    data2.forEach(gcourse => {
        var gcourseItem = createGroupCourseCard(gcourse, true);
        gcontainer.appendChild(gcourseItem);
    });

    // 初始化轮播等其他功能
    initPopularCoursesSlider('#group_course-container');
}
