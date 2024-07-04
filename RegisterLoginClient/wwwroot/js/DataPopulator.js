function PopulatorUser(data) {
    var tablebody = $('#DataTable tbody');
    tablebody.empty();

    data.forEach(user => {
        let row =
            `<tr>
                     <td>${user.id}</td>
                     <td>${user.name}</td>
                     <td>${user.birthday}</td>
                     <td>${user.created_at}</td>
                     <td>${user.google_id}</td>
                     <td>${user.photo}</td>
                     <td>${user.gender == 1 ? "Male" : user.gender == 2 ? "Female" : "Non-binary gender"}</td>
                </tr>`
        tablebody.append(row);
    })
}

function PopulatorSchedule(data) {
    var tablebody = $('#DataTable tbody');
    tablebody.empty();

    data.forEach(schedule => {
        let row =
            `<tr>
                     <td>${schedule.id}</td>
                     <td>${schedule.name}</td>
                     <td>${schedule.start_time}</td>
                     <td>${schedule.end_time}</td>
                     <td>${schedule.created_at}</td>
                     <td>${schedule.user_id}</td>
                </tr>`
        tablebody.append(row);
    })
}

function PopulatorExpense(data) {
    var tablebody = $('#DataTable tbody');
    tablebody.empty();

    data.forEach(expense => {
        let row =
            `<tr>
                     <td>${expense.id}</td>
                     <td>${expense.payer}</td>
                     <td>${expense.name}</td>
                     <td>${expense.schedule}</td>
                     <td>${expense.category}</td>
                     <td>${expense.currency}</td>
                     <td>${expense.amount}</td>
                     <td>${expense.remark}</td>
                </tr>`
        tablebody.append(row);
    })
}

function populateDropdown(selector, data) {
    const dropdown = $(selector);
    dropdown.empty();
    data.forEach(item => {
        dropdown.append(new Option(item.name, item.id));
    });
}