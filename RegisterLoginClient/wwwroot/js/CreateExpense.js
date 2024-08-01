export function initExpenseModal() {
  const { createApp, ref, onMounted } = Vue;

  createApp({
    setup() {
      const expenseName = ref("大成功");

      onMounted(() => {
        $("#CreateExpenseModal").modal("show");
      });

      return {
        expenseName,
      };
    },
  }).mount("#vue-container");
}
