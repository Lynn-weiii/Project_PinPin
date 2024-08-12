export function initExpenseModal() {
  const { createApp, ref, onMounted, computed, nextTick } = Vue;

  createApp({
    setup() {
      const mainForm = ref(null);

      const loading = ref(false);
      const selectedSchedule = ref(null);
      const selectedPayer = ref(null);
      const selectedCurrency = ref(null);
      const selectedCategory = ref(null);
      const selectedBorrowers = ref([]);
      const expenseName = ref("");
      const amount = ref("");
      const remark = ref("");

      const borrowerData = ref([]);
      const decimalPlaces = ref(0);
      const isAvg = ref(true);

      const schedules = ref([]);
      const payers = ref([]);
      const borrowers = ref([]);
      const currencies = ref([]);
      const categories = ref([]);

      const init = async () => {
        $("#CreateExpenseModal").modal("show");
        loading.value = true;
        getRelatedSchedules();
        getCurrencyCategory();
        getSplitCategories();

        loading.value = false;
      };

      //獲取使用者有參加的行程
      const getRelatedSchedules = async () => {
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

      //獲取行程內的成員
      const getScheduleGroups = async () => {
        try {
          let response = await axios.get(
            `${baseAddress}/api/ScheduleGroups/GetScheduleGroups${selectedSchedule.value}`,
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

      //以下為表單相關
      const getBorrowerName = (borrowerId) => {
        const borrowerName = borrowers.value[borrowerId];
        return borrowerName ? borrowerName : "未知借款人";
      };

      const resetBorrowerData = () => {
        borrowerData.value = [];
        selectedBorrowers.value.forEach((item) => {
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
              : borrower.amount > amount.value
              ? amount.value
              : borrower.amount;
          avgTotal();
        });
      };

      const avgTotal = () => {
        const blcokBorrowers = borrowerData.value.filter(
          (borrower) => borrower.lockAmount == true
        );
        const otherBorrowers = borrowerData.value.filter(
          (borrower) => borrower.lockAmount == false
        );

        let filterAmount =
          amount.value -
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

      const setUpDecimal = (num) => {
        const result = decimalPlaces.value + Number(num);
        decimalPlaces.value =
          (result >= 0) & (result <= 4) ? result : decimalPlaces.value;
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
        } else if (amount.value != filterAmount) {
          Swal.fire({
            title: "總分帳金額與總金額不同!",
            text: "請重新檢查金額是否有誤",
            icon: "error",
            confirmButtonText: "OK!",
          });
        } else {
          creatExpense();
        }
      };

      const creatExpense = async () => {
        let formData = {
          scheduleId: selectedSchedule.value,
          payerId: selectedPayer.value,
          splitCategoryId: selectedCategory.value,
          currencyId: selectedCurrency.value,
          name: expenseName.value,
          amount: amount.value,
          remark: remark.value,
          participants: [],
        };

        borrowerData.value.forEach((data) => {
          if (amount != 0) {
            formData.participants.push({
              userId: data.id,
              userName: data.name,
              amount: data.amount,
              isPaid: data.isPaid,
            });
          }
        });

        try {
          let response = await axios.post(
            `${baseAddress}/api/SplitExpenses/PostExpense`,
            formData,
            {
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          Swal.fire({
            title: "分帳表創建成功!!",
            html: `
              <div style="text-align: left;">
                <p>在 <strong>${response.data.schedule}</strong> 行程新增分帳表</p>
                <p><strong>名稱:</strong> ${response.data.name}</p>
                <p><strong>金額:</strong> ${response.data.amount} ${response.data.currency}</p>
              </div>
            `,
            icon: "success",
            confirmButtonText: "OK!",
          }).then((result) => {
            console.log(result.isConfirmed);
            if (result.isConfirmed) {
              const event = new CustomEvent("closeModal", {
                detail: { modalId: "CreateExpenseModal" },
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
        } finally {
        }
      };

      const stepValue = computed(() => {
        return Math.pow(10, -decimalPlaces.value).toFixed(decimalPlaces.value);
      });

      onMounted(() => {
        init();
      });

      return {
        selectedSchedule,
        selectedPayer,
        schedules,
        selectedCurrency,
        selectedCategory,
        selectedBorrowers,
        amount,
        expenseName,
        remark,
        payers,
        borrowers,
        currencies,
        categories,
        getScheduleGroups,
        getBorrowerName,
        loading,
        resetBorrowerData,
        borrowerData,
        decimalPlaces,
        isAvg,
        setUpDecimal,
        stepValue,
        avgTotal,
        handleAmountInput,
        validateForm,
        mainForm,
      };
    },
  }).mount("#vue-container");
}
