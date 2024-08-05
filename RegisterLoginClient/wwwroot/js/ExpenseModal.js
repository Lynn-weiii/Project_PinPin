export function initExpenseModal() {
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

      const payers = ref([]);
      const borrowers = ref([]);
      const currencies = ref([]);
      const categories = ref([]);

      const borrowerData = ref([]);
      const decimalPlaces = ref(0);
      const isAvg = ref(false);
      //------------------------------------------燈箱操控----------------------------------------------------
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

      //------------------------------------------獲取資料----------------------------------------------------
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
      const initDatabase = (code) => {
        return new Promise((resolve, reject) => {
          const request = indexedDB.open("ExchangeRatesDB", 1);

          request.onupgradeneeded = (event) => {
            const db = event.target.result;
            const metaStore = db.createObjectStore("meta", { keyPath: "key" });
          };

          request.onsuccess = (event) => {
            resolve(event.target.result);
          };

          request.onerror = (event) => {
            reject(event.target.error);
          };
        });
      };

      function createObjectStore(db, storeName) {
        return new Promise((resolve, reject) => {
          if (!db.objectStoreNames.contains(storeName)) {
            const version = db.version + 1;
            db.close();
            const request = indexedDB.open(db.name, version);

            request.onupgradeneeded = (event) => {
              const upgradeDb = event.target.result;
              if (!upgradeDb.objectStoreNames.contains(storeName)) {
                const store = upgradeDb.createObjectStore(storeName, {
                  keyPath: "id",
                });
                store.createIndex("currency", "code", { unique: false });
              }
            };

            request.onsuccess = (event) => {
              resolve(event.target.result);
            };

            request.onerror = (event) => {
              reject(event.target.error);
            };
          } else {
            resolve(db);
          }
        });
      }

      const storeExchangeRates = async (code, data) => {
        try {
          const db = await initDatabase();
          const updatedDb = await createObjectStore(db, `${code}rates`);

          const transaction = updatedDb.transaction(
            [`${code}rates`, "meta"],
            "readwrite"
          );
          const rateStore = transaction.objectStore(`${code}rates`);
          const metaStore = transaction.objectStore("meta");

          const today = new Date().toISOString().split("T")[0];

          data.forEach((item) => {
            rateStore.put(item);
          });

          metaStore.put({ key: `${code}lastUpdate`, date: today });

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

      async function getExchangeRates(code) {
        try {
          const db = await initDatabase();
          const transaction = db.transaction(
            [`${code}rates`, "meta"],
            "readonly"
          );
          const rateStore = transaction.objectStore(`${code}rates`);
          const metaStore = transaction.objectStore("meta");

          const lastUpdateRequest = metaStore.get("lastUpdate");
          const ratesRequest = rateStore.getAll();

          return new Promise((resolve, reject) => {
            transaction.oncomplete = () => {
              const lastUpdate = lastUpdateRequest.result;
              const rates = ratesRequest.result;
              resolve({ lastUpdate, rates });
            };

            transaction.onerror = (event) => {
              console.error("Transaction error:", event.target.error);
              reject(event.target.error);
            };
          });
        } catch (error) {
          console.error("Failed to retrieve exchange rates:", error);
        }
      }

      const ensureExchangeRates = async (code) => {
        try {
          const today = new Date().toISOString().split("T")[0];
          const { lastUpdate, rates } = await getExchangeRates(code);
          if (!rates || rates.length === 0 || lastUpdate != today) {
            console.log(`No rates found for code ${code}, storing data...`);
            const response = await axios.get(
              `${baseAddress}/api/ChangeRate/${code}`,
              {
                headers: {
                  Authorization: `Bearer ${token}`,
                },
              }
            );
            const data = await response.data;
            await storeExchangeRates(code, data);
            return data;
          } else {
            console.log(`Rates found for code ${code}`);
            return rates;
          }
        } catch (error) {
          console.error("Error ensuring exchange rates:", error);
        }
      };

      const totalBalance = computed(() => {
        return schedulebalances.value.reduce(
          (sum, balance) => sum + balance.isPaidBalance,
          0
        );
      });

      const totalExpensAmount = computed(() => {
        return expenses.value.reduce((sum, amount) => sum + amount.amount, 0);
      });

      const totalUserBalance = computed(() => {
        const userBalance = schedulebalances.value.find(
          (sb) => sb.userName === userName.value
        );
        return userBalance?.isPaidBalance ?? 0;
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
        totalExpensAmount,
        loading,
        goBack,
        getScheduleBalance,
        getUserExpense,
        getExpense,
        getAllScheduleExpenses,
        createExpense,
        //編輯分帳表
        getEditExpenseData,
        editName,
        editAmount,
        editRemark,
        editCategory,
        editCurrency,
        editPayer,
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
        ensureExchangeRates,
      };
    },
  }).mount("#vue-container");
}
