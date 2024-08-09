export function initExpenseModal() {
  const { createApp, ref, onMounted, nextTick, computed, watch } = Vue;
  createApp({
    setup() {
      const userData = ref(0);
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
      const codeIcons = ref([]);
      //編輯分帳表用
      const mainForm = ref(null);
      const editExpenseId = ref(0);
      const editName = ref([]);
      const editAmount = ref(0);
      const editRemark = ref("");
      const editCategory = ref(null);
      const editCurrency = ref(null);
      const editPayer = ref(null);
      const editBorrowers = ref([]);
      const selectedEditCurrencyIcon = ref("");

      const payers = ref([]);
      const borrowers = ref([]);
      const currencies = ref([]);
      const categories = ref([]);

      const borrowerData = ref([]);
      const decimalPlaces = ref(0);
      const isAvg = ref(false);

      const selectedCurrency = ref("TWD");
      const selectedCurrencyIcon = ref("fa-solid fa-dollar-sign");
      //計算總結用
      const balanceData = ref([]);
      const changeRateExpense = ref([]);
      const userChangeRateExpense = ref([]);
      //------------------------------------------燈箱操控---------------------------------------------------
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

      const showModalWithData = async (modalId, dataFunction = () => {}) => {
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
        var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
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

      const scheduleBalance = async () => {
        balanceData.value = [];
        const expensesList = changeRateExpense.value;
        const { payYourselfData, otherData } = expensesList.reduce(
          (acc, expense) => {
            if (expense.payer === userData.value.name) {
              acc.payYourselfData.push(expense);
            } else {
              acc.otherData.push(expense);
            }
            return acc;
          },
          { payYourselfData: [], otherData: [] }
        );

        payYourselfData.forEach((expense) => {
          expense.expenseParticipants.forEach((ep) => {
            if (ep.userId !== userData.value.id) {
              const existingBalance = balanceData.value.find(
                (b) => b.name === ep.userName
              );
              if (existingBalance) {
                existingBalance.balance += ep.amount;
                if (ep.isPaid == false) {
                  existingBalance.isPaidBalance += ep.amount;
                }
              } else {
                balanceData.value.push({
                  id: ep.userId,
                  name: ep.userName,
                  balance: ep.amount,
                  isPaidBalance: ep.isPaid == false ? ep.amount : 0,
                });
              }
            }
          });
        });

        otherData.forEach((expense) => {
          const ep = expense.expenseParticipants.find(
            (ep) => ep.userId === userData.value.id
          );
          if (ep) {
            const existingBalance = balanceData.value.find(
              (b) => b.name === expense.payer
            );
            if (existingBalance) {
              existingBalance.balance -= ep.amount;
              if (ep.isPaid == false) {
                existingBalance.isPaidBalance -= ep.amount;
              }
            } else {
              balanceData.value.push({
                id: expense.payerId,
                name: expense.payer,
                balance: -ep.amount,
                isPaidBalance: ep.isPaid == false ? -ep.amount : 0,
              });
            }
          }
        });
      };

      //------------------------------------------獲取資料----------------------------------------------------
      const getUserData = async () => {
        try {
          let response = await axios.get(
            `${baseAddress}/api/user/GetUserIdName`,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          userData.value = response.data;
        } catch (error) {
          console.log(error);
        }
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

      const getAllScheduleExpenses = async (id, name) => {
        try {
          scheduleId.value = id;
          scheduleName.value = name;
          let response = await axios.get(
            `${baseAddress}/api/SplitExpenses/GetScheduleIdExpense${scheduleId.value}`,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          expenses.value = response.data;
          changeRates();
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
          changeRates();
        } catch (error) {
          console.log(error);
        }
      };

      const getExpense = async (id) => {
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

      //------------------------------------------編輯資料----------------------------------------------------
      //建立編輯分帳表示窗
      const getEditExpenseData = async () => {
        await getScheduleGroups();
        await getCurrencyCategory();
        await getSplitCategories();
        editName.value = expense.value.name;
        editAmount.value = expense.value.amount;
        editRemark.value = expense.value.remark;
        editExpenseId.value = expense.value.id;

        const PayerId = Object.keys(payers.value).find(
          (key) => payers.value[key] == expense.value.payer
        );
        if (PayerId) {
          editPayer.value = parseInt(PayerId, 10);
        }

        const CategoryId = Object.keys(categories.value).find(
          (key) => categories.value[key] === expense.value.category
        );
        if (CategoryId) {
          editCategory.value = parseInt(CategoryId, 10);
        }

        const CurrencyId = Object.keys(currencies.value).find(
          (key) => currencies.value[key] === expense.value.currency
        );
        if (CurrencyId) {
          editCurrency.value = parseInt(CurrencyId, 10);
        }

        borrowerData.value = [];

        expense.value.expenseParticipants.forEach((item) => {
          borrowerData.value.push({
            id: item.userId,
            name: getBorrowerName(item.userId),
            amount: item.amount,
            isPaid: item.isPaid,
            lockAmount: false,
          });
        });
      };

      const getBorrowerName = (borrowerId) => {
        const borrowerName = borrowers.value[borrowerId];
        return borrowerName ? borrowerName : "未知借款人";
      };

      //獲取行程內的成員
      const getScheduleGroups = async () => {
        try {
          let response = await axios.get(
            `${baseAddress}/api/ScheduleGroups/GetScheduleGroups${scheduleId.value}`,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          payers.value = response.data;
          borrowers.value = response.data;
        } catch (error) {
          console.log(error);
        }
      };

      //獲取幣別種類
      const getCurrencyCategory = async () => {
        try {
          let response = await axios.get(
            `${baseAddress}/api/category/GetCurrencyCategory`,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          currencies.value = response.data;
        } catch (error) {
          console.log(error);
        }
      };

      //獲取花費的種類
      const getSplitCategories = async () => {
        try {
          let response = await axios.get(
            `${baseAddress}/api/category/GetSplitCategories`,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          categories.value = response.data;
        } catch (error) {
          console.log(error);
        }
      };

      //小數位取捨
      const setUpDecimal = (num) => {
        const result = decimalPlaces.value + Number(num);
        decimalPlaces.value =
          (result >= 0) & (result <= 4) ? result : decimalPlaces.value;
      };

      const avgTotal = () => {
        const blcokBorrowers = borrowerData.value.filter(
          (borrower) => borrower.lockAmount == true
        );
        const otherBorrowers = borrowerData.value.filter(
          (borrower) => borrower.lockAmount == false
        );
        let filterAmount =
          editAmount.value -
          blcokBorrowers.reduce(
            (accumulator, currentValue) => accumulator + currentValue.amount,
            0
          );
        const memberCount = otherBorrowers.length;

        if ((isAvg.value === true) & (memberCount >= 0) & (filterAmount >= 0)) {
          let amounts = [];
          //計算不含小數的
          if (decimalPlaces.value == 0) {
            let avgAmount = Math.floor(filterAmount / memberCount);
            let remainder = filterAmount % memberCount;
            amounts = Array(memberCount).fill(avgAmount);

            for (let i = 0; i < memberCount; i++) {
              if (remainder > 0) {
                amounts[i]++;
                remainder--;
              }
            }
          } else {
            const factor = Math.pow(10, decimalPlaces.value);
            const avgAmount = Math.floor((filterAmount / memberCount) * factor);
            const initialTotal = avgAmount * memberCount;
            let remainder = Math.floor(filterAmount * factor - initialTotal);
            let amountsCopy = Array(memberCount).fill(avgAmount);
            for (let i = 0; i < memberCount; i++) {
              if (remainder > 0) {
                amountsCopy[i]++;
                remainder--;
              }
            }
            amounts = amountsCopy.map((amount) => amount / factor);
          }
          otherBorrowers.forEach((item, index) => {
            item.amount = amounts[index];
          });
        }
      };

      const resetBorrowerData = () => {
        borrowerData.value = [];
        editBorrowers.value.forEach((item) => {
          borrowerData.value.push({
            id: item,
            name: getBorrowerName(item),
            amount: 0,
            isPaid: false,
            lockAmount: false,
          });
        });
        avgTotal();
      };

      const handleAmountInput = (borrower) => {
        nextTick(() => {
          borrower.lockAmount = true;
          borrower.amount =
            borrower.amount < 0
              ? 0
              : borrower.amount > editAmount.value
              ? editAmount.value
              : borrower.amount;
          avgTotal();
        });
      };

      const validateForm = async () => {
        const formElement = mainForm.value;
        formElement.classList.remove("was-validated");
        let filterAmount = borrowerData.value.reduce(
          (accumulator, currentValue) => accumulator + currentValue.amount,
          0
        );
        await nextTick();
        if (!formElement.checkValidity()) {
          formElement.classList.add("was-validated");

          setTimeout(() => {
            formElement.classList.remove("was-validated");
          }, 2000);
        } else if (editAmount.value != filterAmount) {
          Swal.fire({
            title: "總分帳金額與總金額不同!",
            text: "請重新檢查金額是否有誤",
            icon: "error",
            confirmButtonText: "OK!",
          });
        } else {
          let response = await axios.delete(
            `${baseAddress}/api/SplitExpenses/DeleteExpense${editExpenseId.value}`,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          putExpense();
        }
      };

      const putExpense = async () => {
        let formData = {
          scheduleId: scheduleId.value,
          payerId: editPayer.value,
          splitCategoryId: editCategory.value,
          currencyId: editCurrency.value,
          name: editName.value,
          amount: editAmount.value,
          remark: editRemark.value,
          participants: [],
        };

        borrowerData.value.forEach((data) => {
          formData.participants.push({
            userId: data.id,
            userName: data.name,
            amount: data.amount,
            isPaid: data.isPaid,
          });
        });

        console.log(formData);

        try {
          let response = await axios.put(
            `${baseAddress}/api/SplitExpenses/UpdateExpense${editExpenseId.value}`,
            formData,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          Swal.fire({
            title: "分帳表更改成功!!",
            icon: "success",
            confirmButtonText: "OK!",
          }).then((result) => {
            if (result.isConfirmed) {
              const event = new CustomEvent("closeModal", {
                detail: { modalId: "EditExpenseModal" },
              });
              window.dispatchEvent(event);
            }
          });
        } catch (error) {
          Swal.fire({
            title: "與伺服器連接失敗",
            text: "請與管理員聯絡",
            icon: "error",
            confirmButtonText: "OK!",
          });
        }
      };

      const deleteExpense = async () => {
        const result = await Swal.fire({
          title: "確認刪除嗎?",
          text: "你確定要刪除這個分帳表嗎?",
          icon: "warning",
          showCancelButton: true,
          confirmButtonText: "删除",
          cancelButtonText: "取消",
        });

        if (result.isConfirmed) {
          try {
            let response = await axios.delete(
              `${baseAddress}/api/SplitExpenses/DeleteExpense${editExpenseId.value}`,
              {
                headers: {
                  Authorization: `Bearer ${token}`,
                },
              }
            );

            if (response.status === 200) {
              Swal.fire({
                title: "删除成功",
                text: "分帳表已刪除!",
                icon: "success",
                confirmButtonText: "確定",
              });

              const event = new CustomEvent("closeModal", {
                detail: { modalId: "EditExpenseModal" },
              });
              window.dispatchEvent(event);
            }
          } catch (error) {
            // 错误处理逻辑
            console.error("Failed to delete expense", error);
            Swal.fire({
              title: "刪除失敗",
              text: "删除過程中發生錯誤，請等下再試看看",
              icon: "error",
              confirmButtonText: "确定",
            });
          }
        }
      };

      const stepValue = computed(() => {
        return Math.pow(10, -decimalPlaces.value).toFixed(decimalPlaces.value);
      });

      //------------------------------------------創建新的分帳表----------------------------------------------------
      const createExpense = () => {
        const event = new CustomEvent("createExpense");
        window.dispatchEvent(event);
      };

      //------------------------------------------存緩存IndexedDB----------------------------------------------------
      const initDatabase = () => {
        return new Promise((resolve, reject) => {
          const request = indexedDB.open("ExchangeRatesDB", 1);

          request.onupgradeneeded = (event) => {
            const db = event.target.result;
            const rateStore = db.createObjectStore("rates", {
              keyPath: "code",
            });
            rateStore.createIndex("currency", "code", { unique: true });
          };

          request.onsuccess = (event) => {
            resolve(event.target.result);
          };

          request.onerror = (event) => {
            reject(event.target.error);
          };
        });
      };

      const storeExchangeRates = async (code, ratedata) => {
        try {
          const db = await initDatabase();
          const transaction = db.transaction(["rates"], "readwrite");
          const rateStore = transaction.objectStore("rates");

          const today = new Date().toISOString().split("T")[0];

          const data = {
            code: code,
            lastUpdate: today,
            data: ratedata,
          };

          rateStore.put(data);

          transaction.oncomplete = () => {
            console.log("All data and update time inserted successfully!");
          };

          transaction.onerror = (event) => {
            console.error("Transaction error:", event.target.error);
          };
        } catch (error) {
          console.error("Failed to store exchange rates:", error);
        }
      };

      const getExchangeRatesb = async (code) => {
        try {
          const db = await initDatabase();
          const transaction = db.transaction(["rates"], "readonly");
          const rateStore = transaction.objectStore("rates");

          return new Promise((resolve, reject) => {
            const request = rateStore.get(code);
            request.onsuccess = async (event) => {
              const today = new Date().toISOString().split("T")[0];
              const result = event.target.result;
              if (result) {
                if (result.lastUpdate === today) {
                  resolve(result);
                } else {
                  const response = await axios.get(
                    `${baseAddress}/api/ChangeRate/${code}`,
                    {
                      headers: {
                        Authorization: `Bearer ${token}`,
                      },
                    }
                  );
                  if (response.status === 200) {
                    const rates = response.data;
                    await storeExchangeRates(code, rates);
                    resolve({
                      code: code,
                      lastUpdate: today,
                      data: rates,
                    });
                  } else
                    reject(
                      new Error("Failed to fetch exchange rates from API")
                    );
                }
              } else {
                const response = await axios.get(
                  `${baseAddress}/api/ChangeRate/${code}`,
                  {
                    headers: {
                      Authorization: `Bearer ${token}`,
                    },
                  }
                );
                if (response.status === 200) {
                  const rates = response.data;
                  await storeExchangeRates(code, rates);
                  resolve({
                    code: code,
                    lastUpdate: today,
                    data: rates,
                  });
                } else
                  reject(new Error("Failed to fetch exchange rates from API"));
              }
            };
            request.onerror = (event) => {
              reject(event.target.error);
            };
          });
        } catch (error) {
          console.error("Failed to get exchange rates:", error);
          return [];
        }
      };

      const calculateChangeRateExpense = (ratedata) => {
        changeRateExpense.value = [];
        expenses.value.forEach((expens) => {
          const rate = ratedata.find(
            (data) => data.code === expens.currency
          ).rate;
          let RateExpenseParticipants = [];
          expens.expenseParticipants.forEach((ep) => {
            let rateep = {
              userId: ep.userId,
              userName: ep.userName,
              amount: parseFloat(ep.amount / rate),
              isPaid: ep.isPaid,
            };
            RateExpenseParticipants.push(rateep);
          });

          let RateExpense = {
            id: expens.id,
            name: expens.name,
            payerId: expens.payerId,
            payer: expens.payer,
            category: expens.category,
            amount: parseFloat(expens.amount / rate),
            expenseParticipants: RateExpenseParticipants,
          };

          changeRateExpense.value.push(RateExpense);
        });
      };

      const calculateUserChangeRateExpense = (ratedata) => {
        userChangeRateExpense.value = [];
        userExpenses.value.forEach((expens) => {
          const rate = ratedata.find(
            (data) => data.code === expens.costCategory
          ).rate;
          let userExpense = {
            expenseId: expens.expenseId,
            expenseName: expens.expenseName,
            expenseCategory: expens.expenseCategory,
            amount: parseFloat(expens.amount / rate),
            costCategory: expens.costCategory,
            isPaid: expens.isPaid,
          };
          userChangeRateExpense.value.push(userExpense);
        });
      };

      const setIcon = (data) => {
        codeIcons.value = [];
        data.forEach((data) => {
          let codeIcon = {
            code: data.code,
            icon: data.icon,
          };
          codeIcons.value.push(codeIcon);
        });
      };

      const changeRates = async () => {
        const ratedata = await getExchangeRatesb(selectedCurrency.value);

        setIcon(ratedata.data);
        calculateChangeRateExpense(ratedata.data);
        calculateUserChangeRateExpense(ratedata.data);
        nextTick(() => {
          scheduleBalance();
        });
      };

      const totalBalance = computed(() => {
        return balanceData.value
          .reduce((sum, balance) => sum + balance.isPaidBalance, 0)
          .toFixed(2);
      });

      const totalExpensAmount = computed(() => {
        return changeRateExpense.value
          .reduce((sum, amount) => sum + amount.amount, 0)
          .toFixed(2);
      });

      const totalUserBalance = computed(() => {
        const userBalance = balanceData.value.find(
          (sb) => sb.name === userName.value
        );
        return userBalance?.isPaidBalance.toFixed(2) ?? 0;
      });

      watch(selectedCurrency, (newValue) => {
        if (newValue) {
          const icon = codeIcons.value.find(
            (code) => code.code === newValue
          ).icon;
          selectedCurrencyIcon.value = icon;
        }
      });

      watch(editCurrency, (newValue) => {
        if (newValue) {
          const icon = codeIcons.value.find(
            (code) => code.code === currencies.value[newValue]
          ).icon;
          selectedEditCurrencyIcon.value = icon;
        }
      });

      onMounted(() => {
        showModalWithData("ScheduleModal", getSchedules);
        getCurrencyCategory();
        getUserData();
      });

      return {
        showModalWithData,
        schedules,
        expenses,
        expense,
        schedulebalances,
        userName,
        scheduleId,
        scheduleName,
        totalBalance,
        totalUserBalance,
        totalExpensAmount,
        loading,
        goBack,
        getUserExpense,
        getExpense,
        getAllScheduleExpenses,
        createExpense,
        scheduleBalance,
        //編輯分帳表
        getEditExpenseData,
        editName,
        editAmount,
        editRemark,
        editCategory,
        editCurrency,
        editPayer,
        selectedEditCurrencyIcon,
        payers,
        borrowers,
        currencies,
        categories,
        setUpDecimal,
        decimalPlaces,
        isAvg,
        avgTotal,
        resetBorrowerData,
        editBorrowers,
        borrowerData,
        stepValue,
        handleAmountInput,
        validateForm,
        mainForm,
        deleteExpense,
        //匯率蕭觀
        selectedCurrency,
        changeRates,
        balanceData,
        changeRateExpense,
        userChangeRateExpense,
        selectedCurrencyIcon,
      };
    },
  }).mount("#vue-container");
}
