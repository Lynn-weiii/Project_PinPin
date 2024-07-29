const baseAddress = "https://localhost:7280";
const token = localStorage.getItem("token");

$(function () {
  $("#btnModalExpense").on("click", async function () {
    $("#modal-container").empty();

    try {
      let partialresponse = await axios.get("/Expense/ModalExpense");
      let data = partialresponse.data;
      $("#modal-container").html(data);

      const { createApp, ref, onMounted } = Vue;

      createApp({
        setup() {
          const schedules = ref([]);
          const expenses = ref([]);
          const loading = ref(true);
          const scheduleId = ref(null);
          const scheduleName = ref("");

          //獲取使用者有加入的行程
          const getSchedules = async () => {
            $("#ScheduleModal").modal("show");
            loading.value = true;
            try {
              let response = await axios.get(
                `${baseAddress}/api/schedules/GetRelatedSchedules`,
                {
                  headers: {
                    Authorization: `Bearer ${token}`,
                  },
                }
              );
              schedules.value = response.data;
            } catch (error) {
              console.log(error);
            } finally {
              loading.value = false;
            }
          };

          //獲取某行程的分帳表
          const getScheduleExpense = async (id, name) => {
            scheduleId.value = id;
            scheduleName.value = name;
            loading.value = true;
            console.log(scheduleId.value);
            try {
              let response = await axios.get(
                `${baseAddress}/api/SplitExpenses/GetBalance${scheduleId.value}`,
                {
                  headers: {
                    Authorization: `Bearer ${token}`,
                  },
                }
              );
              expenses.value = response.data;

              $("#ScheduleModal").modal("hide");
              $("#ExpenseModal").modal("show");
            } catch (error) {
              console.log(error);
            } finally {
              loading.value = false;
            }
          };

          const goBack = () => {
            $("#ExpenseModal").modal("hide");
            getSchedules();
          };
          onMounted(() => {
            getSchedules();
          });

          return {
            schedules,
            expenses,
            loading,
            scheduleId,
            scheduleName,
            getSchedules,
            getScheduleExpense,
            goBack,
          };
        },
      }).mount("#vue-container");
    } catch (error) {
      console.log(error);
    }
  });
});

$("#btnCreateExpense").on("click", async () => {
  try {
    let partialresponse = await axios("/Expense/LoadCreate");

    resetPage(partialresponse);
  } catch (error) {
    console.log(error);
    alert("獲取資料失敗");
  }
});

function ShowDetail() {
  $("#DataTable tbody")
    .off("click", "tr")
    .on("click", "tr", function () {
      let cells = $(this).find("td");
      $("#datadetial").empty();

      cells.each(function (index) {
        let cellContent = $(this).text();
        let headerText = $("#DataTable th").eq(index).text();
        let formGroup = $('<div class="form-group"></div>');
        let label = $("<label></label>")
          .attr("for", headerText)
          .text(headerText);
        let input = $('<input type="text" class="form-control">')
          .attr("id", headerText)
          .val(cellContent);
        formGroup.append(label).append(input);
        $("#datadetial").append(formGroup);
      });

      ShowParticipant($(cells[0]).text());
    });
}

async function ShowParticipant(id) {
  let response = await axios.post(
    `${baseAddress}/api/SplitExpenses/GetExpense_participant`,
    new URLSearchParams({
      id: `${id}`,
    }),
    {
      headers: {
        Authorization: `bearer ${token}`,
      },
    }
  );
  let data = response.data;
  let keys = Object.keys(data[0]);

  let title = $("<h2></h2>").text("詳細內容");
  $("#datadetial").append(title);

  data.forEach((participant) => {
    delete participant["userId"];
    let keys = Object.keys(participant);
    let formGroup = $(
      '<div class="form-group border border-2 border-dark mb-3 p-2 rounded"></div>'
    );
    keys.forEach((key, index) => {
      if (key == "isPaid") {
        console.log(key);
        let formcheck = $('<div class="form-check"></div>');
        let input = $('<input class="form-check-input" type="checkbox" >')
          .attr("id", key)
          .val(participant[key])
          .prop("checked", participant[key] === true);
        let label = $("<label></label>").attr("for", key).text(key);

        formcheck.append(input).append(label);
        formGroup.append(formcheck);
        $("#datadetial").append(formGroup);

        return;
      }
      let label = $("<label></label>").attr("for", key).text(key);
      let input = $('<input type="text" class="form-control">')
        .attr("id", key)
        .val(participant[key]);

      formGroup.append(label).append(input);
      $("#datadetial").append(formGroup);
    });
  });
}
