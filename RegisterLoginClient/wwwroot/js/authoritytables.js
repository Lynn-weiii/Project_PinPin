function populateTable(data) {
    const tableBody = document.getElementById('authoritytable');
    tableBody.innerHTML = '';

    data.forEach(item => {
        const { userId, userName, authorityCategoryIds, scheduleId } = item;
        const row = document.createElement('tr');

        row.innerHTML = `
        <td style="text-align:center; vertical-align: middle;" data-scheduleId="${scheduleId}" data-authorityuserid="${userId}">${userName}
        <a class="remove_btn" data-removememberid="${userId}" style="font-size:12px;"><i class="fa-solid fa-circle-xmark" style="color: #cf072f;"></i></button>
        </td>
        <td style="text-align:center;vertical-align: middle">
        <div class="form-check">
        <input class="form-check-input" type="checkbox" data-authority-id="2" ${authorityCategoryIds.includes(2) ? 'checked' : ''}>
        </div>
        </td>
        <td style="text-align:center;vertical-align: middle">
        <div class="form-check">
        <input class="form-check-input" type="checkbox" data-authority-id="5" ${authorityCategoryIds.includes(5) ? 'checked' : ''}>
        </div>
        </td>
        <td style="text-align:center;vertical-align: middle">
        <div class="form-check">
        <input class="form-check-input" type="checkbox" data-authority-id="3" ${authorityCategoryIds.includes(3) ? 'checked' : ''}>
        </div>
        </td>
        <td style="text-align:center;vertical-align: middle">
        <div class="form-check">
        <input class="form-check-input" type="checkbox" data-authority-id="6" ${authorityCategoryIds.includes(6) ? 'checked' : ''}>
        </div>
        </td>
        <td style="text-align:center;vertical-align: middle">
        <div class="form-check">
        <input class="form-check-input" type="checkbox" data-authority-id="4" ${authorityCategoryIds.includes(4) ? 'checked' : ''}>
        </div>
        </td>
        <td style="text-align:center;vertical-align: middle">
        <div class="form-check">
        <input class="form-check-input" type="checkbox" data-authority-id="7" ${authorityCategoryIds.includes(7) ? 'checked' : ''}>
        </div>
        </td>
        `;

        tableBody.appendChild(row);
    });
}
