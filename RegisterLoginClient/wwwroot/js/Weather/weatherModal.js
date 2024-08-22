export function initWeatherModal() {
  const { createApp, ref, onMounted, watch, nextTick } = Vue;
  createApp({
    setup() {
      const isTableVisible = ref(false);
      const scheduleInfo = ref(null);
      const scheduleName = ref("");
      const weatherData = ref([]);
      const chartInstance = ref(null);
      const countryName = ref("");
      const cityName = ref("");
      const currentWeatherData = ref([]);

      const init = async () => {
        $("#weatherModal").modal("show");
        await getSchedleInfo();
        await getWeatherData();
        await getCurrentWeatherData();
        initWeatherChart();
      };
      const getWeatherData = async () => {
        try {
          let response = await axios.get(`${baseAddress}/api/Weather`, {
            params: {
              lat: scheduleInfo.value.scheduleDetail[0].lat,
              lon: scheduleInfo.value.scheduleDetail[0].lng,
              units: "metric",
            },
            headers: {
              Authorization: `Bearer ${token}`,
            },
          });
          cityName.value = response.data[0].CityName;
          countryName.value = response.data[0].Country;
          weatherData.value = response.data;
        } catch (error) {
          console.log(error);
        }
      };

      const getCurrentWeatherData = async () => {
        try {
          let response = await axios.get(
            `${baseAddress}/api/Weather/GetCurrentWeatherInfo`,
            {
              params: {
                lat: scheduleInfo.value.scheduleDetail[0].lat,
                lon: scheduleInfo.value.scheduleDetail[0].lng,
                units: "metric",
              },
              headers: {
                Authorization: `Bearer ${token}`,
              },
            }
          );
          currentWeatherData.value = response.data;
        } catch (error) {
          console.log(error);
        }
      };

      const getSchedleInfo = async () => {
        const scheduleID = sessionStorage.getItem("scheduleId");
        let response = await axios.get(
          `${baseAddress}/api/Schedules/Entereditdetailsch/${scheduleID}`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        scheduleInfo.value = response.data;
        scheduleName.value = response.data.scheduleDetail[0].name;
      };

      const initWeatherChart = async () => {
        const ctx = document.getElementById("weatherChart").getContext("2d");

        if (chartInstance.value) {
          chartInstance.value.destroy();
        }

        const labels = weatherData.value.map((item) => {
          const date = new Date(item.DateTime);
          const isMorning = item.IsMorning ? "AM" : "PM";
          return `${date.getMonth() + 1}/${date.getDate()} ${isMorning}`;
        });

        const temperatures = weatherData.value.map((item) => item.Temp);
        const chanceOfRain = weatherData.value.map(
          (item) => item.ChanceOfRain * 100
        );
        const gradient = ctx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, "rgba(67, 90, 79, 0.5)");
        gradient.addColorStop(1, "rgba(67, 90, 79, 0)");

        const rainGradient = ctx.createLinearGradient(0, 0, 0, 400);
        rainGradient.addColorStop(0, "rgba(195, 129, 84, 0.5)");
        rainGradient.addColorStop(1, "rgba(195, 129, 84, 0)");
        chartInstance.value = new Chart(ctx, {
          type: "line",
          data: {
            labels: labels,
            datasets: [
              {
                label: "溫度 (°C)",
                data: temperatures,
                borderColor: "rgb(67, 90, 79)",
                backgroundColor: gradient,
                pointBackgroundColor: "rgb(67, 90, 79)",
                pointRadius: 5,
                pointHoverRadius: 7,
                tension: 0.4,
                yAxisID: "y-temp",
                fill: true,
              },
              {
                label: "降雨機率 (%)",
                data: chanceOfRain,
                borderColor: "rgb(195, 129, 84)",
                backgroundColor: rainGradient,
                pointBackgroundColor: "rgb(195, 129, 84)",
                pointRadius: 5,
                pointHoverRadius: 7,
                tension: 0.4,
                yAxisID: "y-rain",
                fill: true,
              },
            ],
          },
          options: {
            responsive: true,
            animation: {
              duration: 500,
              easing: "easeInOutBounce",
            },
            plugins: {
              tooltip: {
                callbacks: {
                  title: function (tooltipItems) {
                    return tooltipItems[0].label;
                  },
                  label: function (tooltipItem) {
                    let index = tooltipItem.dataIndex;
                    let temp = weatherData.value[index].Temp.toFixed(1);
                    let chanceOfRain = (
                      weatherData.value[index].ChanceOfRain * 100
                    ).toFixed(2);
                    let windSpeed =
                      weatherData.value[index].WindSpeed.toFixed(1);
                    let humidity = weatherData.value[index].Humidity;
                    return [
                      `溫度: ${temp} °C`,
                      `降雨機率: ${chanceOfRain}%`,
                      `風速: ${windSpeed} km/h`,
                      `濕度: ${humidity}%`,
                    ];
                  },
                },
              },
              title: {
                display: true,
                text: "天氣預報",
                font: {
                  size: 24,
                  weight: "bold",
                },
                color: "rgb(131, 80, 33)",
              },
              legend: {
                labels: {
                  font: {
                    size: 16,
                  },
                  color: "rgb(131, 80, 33)",
                },
              },
            },
            scales: {
              "y-temp": {
                type: "linear",
                position: "left",
                ticks: {
                  beginAtZero: true,
                  color: "rgb(131, 80, 33)",
                  font: {
                    size: 12,
                  },
                },
                grid: {
                  color: "rgba(131, 80, 33, 0.2)",
                  borderDash: [3, 3],
                },
              },
              "y-rain": {
                type: "linear",
                position: "right",
                ticks: {
                  beginAtZero: true,
                  color: "rgb(131, 80, 33)",
                  font: {
                    size: 12,
                  },
                },
                grid: {
                  drawOnChartArea: false,
                },
              },
              x: {
                ticks: {
                  color: "rgb(131, 80, 33)",
                  font: {
                    size: 12,
                  },
                },
                grid: {
                  color: "rgba(131, 80, 33, 0.2)",
                  borderDash: [3, 3],
                },
              },
            },
          },
        });
      };

      const toggleView = () => {
        isTableVisible.value = !isTableVisible.value;
      };

      watch(isTableVisible, (newVal) => {
        if (!newVal) {
          nextTick(() => {
            initWeatherChart();
          });
        }
      });

      onMounted(() => {
        init();
      });
      return {
        isTableVisible,
        toggleView,
        scheduleInfo,
        scheduleName,
        weatherData,
        cityName,
        countryName,
        currentWeatherData,
      };
    },
  }).mount("#weather-container");
}
