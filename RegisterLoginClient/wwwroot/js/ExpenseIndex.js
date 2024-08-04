const baseAddress = "https://localhost:7280";
const token = localStorage.getItem("token");

$(function () {
  $("#btnModalExpense").on("click", async function () {
    $("#modal-container").empty();

    try {
      let partialresponse = await axios.get("/Expense/ExpenseModal");
      let data = partialresponse.data;
      $("#modal-container").html(data);

      await Vue.nextTick();

      const module = await import("/js/expenseModal.js");
      module.initExpenseModal();
    } catch (error) {
      console.log(error);
    }
  });

  window.addEventListener("closeModal", (event) => {
    const modalId = event.detail.modalId;
    const modalElement = document.getElementById(modalId);

    if (modalElement) {
      const bsModal = bootstrap.Modal.getInstance(modalElement);

      if (bsModal) {
        bsModal.hide();
      }

      modalElement.addEventListener("hidden.bs.modal", () => {
        modalElement.parentNode.removeChild(modalElement);
      });
    }
  });

  window.addEventListener("createExpense", async function () {
    $("#modal-container").empty();

    try {
      let partialresponse = await axios.get("/Expense/CreatExpense");
      let data = partialresponse.data;
      $("#modal-container").html(data);

      await Vue.nextTick();

      const module = await import("/js/CreateExpense.js");
      module.initExpenseModal();
    } catch (error) {
      console.log(error);
    }
  });
});
