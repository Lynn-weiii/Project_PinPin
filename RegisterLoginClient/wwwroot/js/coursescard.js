function createCourseItem(course) {
    var courseItem = document.createElement('div');
    courseItem.className = 'item course_card_owl_item';
    courseItem.innerHTML = `
    <a href="#" class="course_card_link">
       <div class="course_card">
            <div class="course_card_container">
                <div class="course_card_top">
                    <div class="course_card_category trans_200"></div>
                    <div class="course_card_pic">
                        <img src="/images/caourse/course_03.jpg">
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

function renderCourses(data) {
    var container = document.getElementById('course-container');
    container.innerHTML = ''; // Clear existing content

    data.forEach(course => {
        var courseItem = createCourseItem(course);
        container.appendChild(courseItem);
    });

    // Create the "Add New" card after all courses have been rendered
    var addNewCard = document.createElement('div');
    addNewCard.className = 'item course_card_owl_item';
    addNewCard.innerHTML = `
    <div class="course_card" id="addnewcard">
        <div class="course_card_container">
            <br><br><br><br>
            <div class="course_card_pic" style="py-5">
                <a href="javascript:;" class="add_btn" data-bs-toggle="modal" data-bs-target="#newschdule">
                    <img src="/images/add_new.png" alt="Add New">
                </a>
                <br><br><br><br>
            </div>
        </div>
    </div>
    `;
    container.appendChild(addNewCard);

    initPopularCoursesSlider();
}

function serachCourses(data) {
    var container = document.getElementById('course-container');
    container.innerHTML = ''; // Clear existing content

    data.forEach(course => {
        var searchItem = createCourseItem(course);
        container.appendChild(searchItem);
    });

    initPopularCoursesSlider();
}