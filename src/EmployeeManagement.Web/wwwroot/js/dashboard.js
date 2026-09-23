document.addEventListener("DOMContentLoaded", () => {

    createEmployeeGrowthChart();

    createDepartmentChart();

});


function createEmployeeGrowthChart() {

    const canvas =
        document.getElementById("employeeGrowthChart");

    if (!canvas) {
        return;
    }

    new Chart(canvas, {

        type: "line",

        data: {

            labels: [
                "Oct",
                "Nov",
                "Dec",
                "Jan",
                "Feb",
                "Mar",
                "Apr",
                "May",
                "Jun",
                "Jul",
                "Aug",
                "Sep"
            ],

            datasets: [

                {
                    label: "Employees",

                    data: [
                        1080,
                        1095,
                        1110,
                        1124,
                        1142,
                        1160,
                        1175,
                        1190,
                        1204,
                        1219,
                        1235,
                        1248
                    ],

                    borderWidth: 3,

                    tension: 0.4,

                    fill: true,

                    backgroundColor:
                        "rgba(79, 70, 229, 0.08)",

                    borderColor:
                        "#4f46e5",

                    pointRadius: 3,

                    pointHoverRadius: 5
                }

            ]

        },

        options: {

            responsive: true,

            maintainAspectRatio: false,

            plugins: {

                legend: {
                    display: false
                }

            },

            scales: {

                y: {

                    beginAtZero: false,

                    grid: {
                        color: "rgba(148, 163, 184, 0.15)"
                    }

                },

                x: {

                    grid: {
                        display: false
                    }

                }

            }

        }

    });

}


function createDepartmentChart() {

    const canvas =
        document.getElementById("departmentChart");

    if (!canvas) {
        return;
    }

    new Chart(canvas, {

        type: "doughnut",

        data: {

            labels: [
                "IT",
                "HR",
                "Finance",
                "Operations",
                "Sales"
            ],

            datasets: [

                {
                    data: [
                        350,
                        180,
                        220,
                        310,
                        188
                    ],

                    borderWidth: 0
                }

            ]

        },

        options: {

            responsive: true,

            maintainAspectRatio: false,

            cutout: "68%",

            plugins: {

                legend: {

                    position: "bottom",

                    labels: {

                        usePointStyle: true,

                        padding: 15

                    }

                }

            }

        }

    });

}