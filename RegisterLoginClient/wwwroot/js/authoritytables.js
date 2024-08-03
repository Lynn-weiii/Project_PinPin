function createAuthorityTables(userData) {
const { userId, userName, authorityCategoryIds } = userData;
console.log(`authtables_${JSON.stringify(userData)}`);
const Item = document.createElement('tr');
Item.innerHTML = `
        <td style="font-size:12px;" data-id="${userId}">${userName}</td>
        <td>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" data-authority-id="1" ${authorityCategoryIds.includes(1) ? 'checked' : ''}>
            </div>
        </td>
        <td>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" data-authority-id="4" ${authorityCategoryIds.includes(4) ? 'checked' : ''}>
            </div>
        </td>
        <td>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" data-authority-id="2" ${authorityCategoryIds.includes(2) ? 'checked' : ''}>
            </div>
        </td>
        <td>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" data-authority-id="5" ${authorityCategoryIds.includes(5) ? 'checked' : ''}>
            </div>
        </td>
        <td>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" data-authority-id="3" ${authorityCategoryIds.includes(3) ? 'checked' : ''}>
            </div>
        </td>
        <td>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" data-authority-id="6" ${authorityCategoryIds.includes(6) ? 'checked' : ''}>
            </div>
        </td>
    `;
    return Item;
}

function populateTable(data) {
    const table = document.getElementById('authoritytable');

    // 清空表格内容
    table.innerHTML = '';

    // 生成表格行并将其添加到表格中
    data.forEach(userData => {
        const row = createAuthorityTables(userData);
        table.appendChild(row);
    });
}

