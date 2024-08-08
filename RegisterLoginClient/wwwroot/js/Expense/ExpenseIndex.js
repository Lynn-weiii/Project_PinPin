/*
使用方法:
bottonId改為要觸發的btnId
modalContainerId改為要放燈箱的div區塊
請再引用的html裡增加以下變數
const baseAddress = "https://localhost:7280";
const token = localStorage.getItem("token");
const bottonId = 'btnModalExpense';
const modalContainerId = 'modal-container';
*/

$(function () {
    $(`#${bottonId}`).on("click", async function () {
        $(`#${modalContainerId}`).empty();

        try {
            let partialresponse = await axios.get("/Expense/ExpenseModal");
            let data = partialresponse.data;
            $(`#${modalContainerId}`).html(data);

            await Vue.nextTick();

            const module = await import("/js/Expense/expenseModal.js");
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
        $(`#${modalContainerId}`).empty();

        try {
            let partialresponse = await axios.get("/Expense/CreatExpense");
            let data = partialresponse.data;
            $(`#${modalContainerId}`).html(data);

            await Vue.nextTick();

            const module = await import("/js/Expense/CreateExpense.js");
            module.initExpenseModal();
        } catch (error) {
            console.log(error);
        }
    });
});
