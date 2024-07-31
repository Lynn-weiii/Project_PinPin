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
          const expense = ref([]);
          const schedulebalances = ref([]);
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

          //獲取某行程的結算表
          const getScheduleBalance = async (id, name) => {
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
              schedulebalances.value = response.data;
            } catch (error) {
              console.log(error);
            }
          };

          const getAllScheduleExpenses = async () => {
            try {
              let response = await axios.get(
                `${baseAddress}/api/SplitExpenses/GetScheduleIdExpense${scheduleId.value}`,
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

          //獲取某行程某使用者的分帳表
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

          const getExpense = async () => {
            try {
              let response = await axios.get(
                `${baseAddress}/api/SplitExpenses/GetExpense${id}`,
                {
                  headers: {
                    Authorization: `Bearer ${token}`,
                  },
                }
              );
              expense.value = response.data;
            } catch (error) {
              console.log(error);
            }
          };

          const totalBalance = computed(() => {
            return schedulebalances.value.reduce(
              (sum, balance) => sum + balance.isPaidBalance,
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
            expense,
            schedulebalances,
            userExpenses,
            userName,
            scheduleId,
            scheduleName,
            totalBalance,
            totalUserBalance,
            loading,
            goBack,
            getScheduleBalance,
            getUserExpense,
            getExpense,
            getAllScheduleExpenses,
          };
        },
      }).mount("#vue-container");
    } catch (error) {
      console.log(error);
    }
  });
});
