function iw_SetApproval(storeID, webID, listID, itemID, rblID, txtID) {
    var store = document.getElementById(storeID);
    var status = "Pending";
    var approved = document.getElementById(rblID + "_0");
    if (approved.checked)
        status = "Approved";
    else {
        var rejected = document.getElementById(rblID + "_1");
        if (rejected.checked)
            status = "Denied";
    }

    var txt = document.getElementById(txtID);
    var comment = txt.value;
    comment.replace(/;/g, "<!IW:SC>");
    store.value = webID + "|" + listID + "|" + itemID + "|" + status + "|" + comment;
}
var iw_selectAllApprovalStatus = true;
function iw_selectAllApproval(btn) {
    tbl = btn.parentNode.parentNode.parentNode;
    for (var i = 1; i < tbl.childNodes.length; i++) {
        var chk = iw_getCheckBox(tbl.childNodes[i]);
        if (chk != null)
            chk.checked = iw_selectAllApprovalStatus;
    }

    iw_selectAllApprovalStatus = !iw_selectAllApprovalStatus;
}
function iw_getFirstChild(el) {
    for (var i = 0; i < el.childNodes.length; i++)
        if (el.childNodes[i].nodeType == 1)
            return el.childNodes[i];

    return null;
}
function iw_ApproveRejectAll(tblID, storeID, status) {
    var tbl = iw_getFirstChild(document.getElementById(tblID));
    var store = document.getElementById(storeID);
    store.value = "";

    for (var i = 1; i < tbl.childNodes.length; i++) {
        var chk = iw_getCheckBox(tbl.childNodes[i]);
        if (chk != null && chk.checked) {
            var item = chk.id.split('iwaccheck_')[1] + "|" + status;
            if (store.value != "")
                item = ";" + item;
            store.value += item;
        }
    }

    return store.value != "";
}

function iw_getCheckBox(tr) {
    if (tr.childNodes.length < 2)
        return null;
    for (var i = 0; i < 2; i++) {
        var td = tr.childNodes[i];
        var chk = iw_getFirstChild(td);
        if (chk != null && chk.tagName == "INPUT")
            return chk;
    }
    return null;
}