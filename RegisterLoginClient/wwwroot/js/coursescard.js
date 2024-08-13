
function createCourseCard(course, isGroup = false) {
    var courseItem = document.createElement('div');
    courseItem.className = 'item course_card_owl_item';
    var picture = course.picture ? course.picture : '/images/course/course_01.jpg';
    var detailsUrl = `https://localhost:7215/Schdules/ScduleDetials/${course.id}`;
    courseItem.innerHTML = courseItem.innerHTML = `    
    <div class="course_card">
    <div class="course_card_container">
        <div class="course_card_top">
            <div class="course_menu_section">
                <div class="dropdown">
                    <div class="course_card_category trans_200" type="button" id="courseDropdown-${course.id}" data-sche="${course.id}" data-bs-toggle="dropdown" aria-expanded="false">
                        <i class="fa fa-ellipsis"></i>
                    </div>
                    <ul class="dropdown-menu" aria-labelledby="courseDropdown-${course.id}">
                        <li><a class="memberdrop dropdown-item"  data-fun="MemberManager" data-id="${course.id}" onclick="" data-name="${course.name}">成員管理</a></li>
                        <li><a class="memberdrop dropdown-item" data-fun="InviteMember" data-id="${course.id}" data-name="${course.name}" data-bs-toggle="modal" data-bs-target="#invitemember_modal">邀請成員</a></li>
                        <li><a class="memberdrop dropdown-item" data-fun="Delete" data-id="${course.id}" data-name="${course.name}">刪除行程</a></li>
                    </ul>
                </div>
            </div>            
                <a type="buttom" class="course_card_link edit_btn" data-fun="EditMainSch" data-id="${course.id}">
                <div class="course_card_pic">
                    <img src="${picture}" style="width:430px;height:286px">
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
            </a>
        </div>
    </div>
</div>
    `;
    return courseItem;
}


function createGroupCourseCard(gcourse) {
    var gcourseItem = document.createElement('div');
    gcourseItem.className = 'item course_card_owl_item';
    var gpicture = gcourse.picture ? gcourse.picture : '/images/course/course_01.jpg';
    
    gcourseItem.innerHTML = `
        <div class="course_card">
            <div class="course_card_container">
                <div class="course_card_top">
                    <div class="course_menu_section">
                        <div class="dropdown">
                            <div class="course_card_category trans_200" type="button" id="courseDropdown-${gcourse.id}" data-gsche="${gcourse.id}" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fa fa-ellipsis"></i>
                            </div>
                            <ul class="dropdown-menu" aria-labelledby="courseDropdown-${gcourse.id}">
                                <li><a class="memberdrop dropdown-item" data-fun="CheckManager" data-id="${gcourse.id}" data-name="${gcourse.name}">查看成員</a></li>
                                <li><a class="memberdrop dropdown-item" data-fun="Exit" data-exitid="${gcourse.userId}" data-name="${gcourse.name}">離開</a></li>
                            </ul>
                        </div>
                    </div>
                    <a href="#" class="course_card_link edit_btn" data-fun="EditGroupSch" data-id="${gcourse.id}">
                        <div class="course_card_pic">
                            <img src="${gpicture}" style="width:430px;height:286px">
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
                    </a>
                </div>
            </div>
        </div>
    `;
    return gcourseItem;
}


function renderCourses(data) {
    var container = document.getElementById('course-container');
    container.innerHTML = '';

    data.forEach(course => {
        var courseItem = createCourseCard(course);
        container.appendChild(courseItem);

        // 初始化 Bootstrap
        var dropdownToggleList = [].slice.call(courseItem.querySelectorAll('[data-bs-toggle="dropdown"]'));
        dropdownToggleList.map(function (dropdownToggleEl) {
            return new bootstrap.Dropdown(dropdownToggleEl);
        });
    });
    
  
    initPopularCoursesSlider('#course-container');
}


function groupCourses(data2) {
    var gcontainer = document.getElementById('group_course-container');
    gcontainer.innerHTML = '';
    data2.forEach(gcourse => {
        var gcourseItem = createGroupCourseCard(gcourse, true);
        gcontainer.appendChild(gcourseItem);
    });

    initPopularCoursesSlider('#group_course-container');
}
