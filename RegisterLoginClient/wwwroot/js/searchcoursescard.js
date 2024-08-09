function serachCourses(data) {
    var serachcontainer = document.getElementById('course-container');
    serachcontainer.innerHTML = ''; // Clear existing content

    data.forEach(serachcourse => {
        var serachItem = document.createElement('div');
        serachItem.className = 'item course_card_owl_item';
        serachItem.innerHTML = `
        <a href="#" class="course_card_link">
           <div class="course_card">
                <div class="course_card_container">
                    <div class="course_card_top">
                        <div class="course_card_category trans_200">
                        </div>
                        <div class="course_card_pic">
                            <img src="/images/caourse/course_03.jpg" >
                        </div>
                        <div class="course_card_content">
                            <div class="course_card_meta d-flex flex-row align-items-center">
                            </div>
                            <div class="course_card_title">
                                <h3>${serachcourse.name}</h3>
                            </div>
                            <div class="course_card_author">
                                <span>by ${serachcourse.userName}</span>
                            </div>
                            <div class="course_card_rating d-flex flex-row align-items-center">
                                <h5>${serachcourse.startTime}</h5>
                                <h5 style="padding:5px;"><i class="fa-solid fa-arrow-right" style="color: #0e4e3b;"></i></h5>
                                <h5>${serachcourse.endTime}</h5> <!-- Use course endTime -->
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </a>
        `;
        container.appendChild(serachItem);

    });

    initPopularCoursesSlider();
}