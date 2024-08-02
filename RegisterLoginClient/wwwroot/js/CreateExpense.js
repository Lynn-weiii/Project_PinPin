export function initExpenseModal() {
  const { createApp, ref, onMounted, computed } = Vue;

  createApp({
    setup() {
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
      const isAvg = ref(false);

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

      const getBorrowerName = (borrowerId) => {
        const borrowerName = borrowers.value[borrowerId];
        return borrowerName ? borrowerName : "未知借款人";
      };

      const resetBorrowerData = () => {
        borrowerData.value = [];
        const avgAmount = amount.value / selectedBorrowers.value.length;
        selectedBorrowers.value.forEach((item) => {
          borrowerData.value.push({
            id: item,
            name: getBorrowerName(item),
            amount: 0,
            isPaid: false,
            isBlock: false,
          });
        });
        avgTotal();
      };

      // const avgTotal = () => {
      //   if (isAvg.value === true) {
      //     const memberCount = borrowerData.value.length;
      //     const factor = Math.pow(10, decimalPlaces.value);
      //     console.log(factor);
      //     const avgAmount =
      //       Math.floor((amount / borrowerData.value.length) * factor) / factor;
      //     const initialTotal = avgAmount * memberCount;
      //     const remainingAmount =
      //       Math.round((amount - initialTotal) * factor) / factor;
      //     borrowerData.value.forEach((item, index) => {
      //       item.amount = avgAmount;
      //       if (index < Math.round(remainingAmount * factor)) {
      //         item.amount += 1 / factor;
      //       }
      //     });
      //   }
      // };

      const avgTotal = () => {
        const memberCount = borrowerData.value.length;
        if ((isAvg.value === true) & (memberCount >= 0)) {
          let amounts = [];
          if (decimalPlaces.value == 0) {
            let avgAmount = Math.floor(amount.value / memberCount);
            let remainder = amount.value % memberCount;
            amounts = Array(memberCount).fill(avgAmount);

            for (let i = 0; i < memberCount; i++) {
              if (remainder > 0) {
                amounts[i]++;
                remainder--;
              }
            }
          } else {
            const factor = Math.pow(10, decimalPlaces.value);
            const avgAmount = Math.floor((amount.value / memberCount) * factor);
            console.log(`avgAmount: ${avgAmount}`);
            const initialTotal = avgAmount * memberCount;
            console.log(`initialTotal: ${initialTotal}`);
            let remainder = Math.floor(amount.value * factor - initialTotal);
            console.log(`remainder: ${remainder}`);
            let amountsCopy = Array(memberCount).fill(avgAmount);
            for (let i = 0; i < memberCount; i++) {
              if (remainder > 0) {
                amountsCopy[i]++;
                remainder--;
              }
            }
            amounts = amountsCopy.map((amount) => amount / factor);
          }
          borrowerData.value.forEach((item, index) => {
            item.amount = amounts[index];
          });
        }
      };

      const setUpDecimal = (num) => {
        const result = decimalPlaces.value + Number(num);
        decimalPlaces.value =
          (result >= 0) & (result <= 4) ? result : decimalPlaces.value;
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
      };
    },
  }).mount("#vue-container");
}
