const baseAddress = "https://localhost:7280";
const token = localStorage.getItem("token");

$(function () {
  $("#btnModalExpense").on("click", async function () {
    $("#modal-container").empty();

    try {
      let partialresponse = await axios.get("/Expense/ModalExpense");
      let data = partialresponse.data;
      $("#modal-container").html(data);

      const { createApp, ref, onMounted, nextTick, computed } = Vue;

      createApp({
        setup() {
          const schedules = ref([]);
          const expenses = ref([]);
          const userExpenses = ref([]);
          const loading = ref(true);
          const scheduleId = ref(null);
          const scheduleName = ref("");
          const userName = ref("");
          const modalStack = ref([]);

          const getCurrentModalId = () => {
            return modalStack.value.length
              ? modalStack.value[modalStack.value.length - 1]
              : null;
          };

          const getPreviousModalId = () => {
            return modalStack.value.length > 1
              ? modalStack.value[modalStack.value.length - 2]
              : null;
          };

          const showModal = (modalId) => {
            modalStack.value.push(modalId);
            $(`#${getCurrentModalId()}`).modal("show");
          };

          const hideModal = () => {
            if (getCurrentModalId() !== null) {
              $(`#${getCurrentModalId()}`).modal("hide");
            }
          };

          const SwitchModal = (modalId) => {
            hideModal();
            showModal(modalId);
          };

          const showModalWithData = async (modalId, dataFunction) => {
            SwitchModal(modalId);
            loading.value = true;
            try {
              await dataFunction();
            } catch (error) {
              console.log(error);
            } finally {
              loading.value = false;
              nextTick(() => {
                init();
              });
            }
          };

          //初始化彈出視窗
          const init = () => {
            var popoverTriggerList = [].slice.call(
              document.querySelectorAll('[data-bs-toggle="popover"]')
            );
            var popoverList = popoverTriggerList.map(function (
              popoverTriggerEl
            ) {
              return new bootstrap.Popover(popoverTriggerEl);
            });
          };

          const goBack = () => {
            let PmodalId = getPreviousModalId();
            let CmodalId = getCurrentModalId();
            modalStack.value.pop();
            modalStack.value.pop();
            $(`#${CmodalId}`).modal("hide");
            showModal(PmodalId);
          };

          //獲取使用者有加入的行程
          const getSchedules = async () => {
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
            }
          };

          //獲取某行程的分帳表
          const getScheduleExpense = async (id, name) => {
            scheduleId.value = id;
            scheduleName.value = name;
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
            } catch (error) {
              console.log(error);
            }
          };

          //獲取某行程的分帳表
          const getUserExpense = async (name, memberid) => {
            userName.value = name;
            try {
              let response = await axios.get(
                `${baseAddress}/api/SplitExpenses/GetUserExpense${scheduleId.value}&${memberid}`,
                {
                  headers: {
                    Authorization: `Bearer ${token}`,
                  },
                }
              );
              userExpenses.value = response.data;
            } catch (error) {
              console.log(error);
            }
          };

          const totalBalance = computed(() => {
            return expenses.value.reduce(
              (sum, expens) => sum + expens.isPaidBalance,
              0
            );
          });

          const totalUserBalance = computed(() => {
            return userExpenses.value.reduce(
              (sum, expens) => sum + expens.amount,
              0
            );
          });

          onMounted(() => {
            showModalWithData("ScheduleModal", getSchedules);
          });

          return {
            showModalWithData,
            schedules,
            expenses,
            userExpenses,
            userName,
            scheduleId,
            scheduleName,
            totalBalance,
            totalUserBalance,
            loading,
            goBack,
            getScheduleExpense,
            getUserExpense,
            getScheduleExpense,
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

$("#btnGetExpense").on("click", () => {});

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
