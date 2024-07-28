const baseAddress = "https://localhost:7280";
const token = localStorage.getItem('token');

$("#btnModalExpense").on("click", async () => {
    try {
        let partialresponse = await axios("/Expense/ModalExpense")
        
        $('#exampleModal').modal('show');
    } catch (error) {
        console.log(error);
        alert("獲取資料失敗");
    }
})

$("#btnGetExpense").on("click", async () => {
    try {
        let partialresponse = await axios("/Expense/LoadExpense")

        resetPage(partialresponse)

        let response = await axios.post(`${baseAddress}/api/SplitExpenses/GetAllExpense`, {}, {
            headers: {
                'Authorization': `bearer ${token}`
            }
        })
        let expense = response.data;
        PopulatorExpense(expense);

        ShowDetail();
    } catch (error) {
        console.log(error);
        alert("獲取資料失敗");
    }
})

$("#btnCreateExpense").on("click", async () => {
    try {
        let partialresponse = await axios("/Expense/LoadCreate")

        resetPage(partialresponse)

    } catch (error) {
        console.log(error);
        alert("獲取資料失敗");
    }
})

function ShowDetail() {
    $('#DataTable tbody').off('click', 'tr').on('click', 'tr', function () {
        let cells = $(this).find('td');
        $('#datadetial').empty();

        cells.each(function (index) {
            let cellContent = $(this).text();
            let headerText = $('#DataTable th').eq(index).text();
            let formGroup = $('<div class="form-group"></div>');
            let label = $('<label></label>').attr('for', headerText).text(headerText);
            let input = $('<input type="text" class="form-control">')
                .attr('id', headerText)
                .val(cellContent);
            formGroup.append(label).append(input);
            $('#datadetial').append(formGroup);
        })

        ShowParticipant($(cells[0]).text())
    })
}

async function ShowParticipant(id) {
    let response = await axios.post(`${baseAddress}/api/SplitExpenses/GetExpense_participant`, new URLSearchParams(
        {
            id: `${id}`,
        }), {
        headers: {
            'Authorization': `bearer ${token}`
        }
    });
    let data = response.data;
    let keys = Object.keys(data[0]);

    let title = $('<h2></h2>').text("詳細內容");
    $('#datadetial').append(title);

    data.forEach(participant => {
        delete participant['userId']
        let keys = Object.keys(participant);
        let formGroup = $('<div class="form-group border border-2 border-dark mb-3 p-2 rounded"></div>');
        keys.forEach((key, index) => {

            if (key == 'isPaid') {
                console.log(key)
                let formcheck = $('<div class="form-check"></div>');
                let input = $('<input class="form-check-input" type="checkbox" >').attr('id', key).val(participant[key]).prop('checked', participant[key] === true);
                let label = $('<label></label>').attr('for', key).text(key);

                formcheck.append(input).append(label);
                formGroup.append(formcheck);
                $('#datadetial').append(formGroup);

                return;
            }
            let label = $('<label></label>').attr('for', key).text(key);
            let input = $('<input type="text" class="form-control">').attr('id', key).val(participant[key]);

            formGroup.append(label).append(input);
            $('#datadetial').append(formGroup);
        });
    });
}