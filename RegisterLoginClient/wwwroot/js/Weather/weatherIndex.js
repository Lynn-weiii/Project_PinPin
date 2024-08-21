$(function () {
    $(`#${WeatherBtnId}`).on("click", async function () {
        $(`#${WeatherModalContainerId}`).empty();

        try {
            let partialresponse = await axios.get("/Schdules/WeatherModal");
            let data = partialresponse.data;
            $(`#${WeatherModalContainerId}`).html(data);

            await Vue.nextTick();

            const module = await import("/js/Weather/weatherModal.js");
            module.initWeatherModal();
        } catch (error) {
            console.log(error);
        }
    });
})