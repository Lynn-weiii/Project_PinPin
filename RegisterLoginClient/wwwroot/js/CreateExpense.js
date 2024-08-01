export function initExpenseModal() {
  const { createApp, ref, onMounted } = Vue;

  createApp({
    setup() {
      const loading = ref(false);
      const selectedSchedule = ref(null);
      const selectedPayer = ref(null);
      const selectedCurrency = ref(null);
      const selectedCategory = ref(null);
      const selectedBorrowers = ref([]);

      const schedules = ref([]);
      const payers = ref([]);
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
        payers,
        currencies,
        categories,
        getScheduleGroups,
      };
    },
  }).mount("#vue-container");
}
