const PATH_ADMIN = "/admin";

/* ============ alert =========== */
function alert(type = "success", message) {
    const alertDiv = document.createElement("div");
    alertDiv.classList.add("alert-container", type);
    let icon = `<i class="fa-solid fa-circle-info"></i>`;
    if (type == "error") {
        icon = `<i class="fa-solid fa-bug"></i>`;
    } else if (type == "warning") {
        icon = `<i class="fa-solid fa-triangle-exclamation"></i>`;
    }
    alertDiv.innerHTML = `
                            <span class="alert-icon">
                                ${icon}
                            </span>
                            <p class="alert-content">${message}</p>
                            <span class="alert-icon-close">
                                <i class="fa-solid fa-xmark"></i>
                            </span>
                        `;
    document.querySelector("body").appendChild(alertDiv);
    setTimeout(() => {
        alertDiv.classList.add("active");
        alertDiv.querySelector(".alert-icon-close").onclick = () => {
            alertDiv.classList.remove("active");
        };
        setTimeout(() => {
            alertDiv.classList.remove("active");
        }, 5000);

        setTimeout(() => {
            if (alertDiv.parentNode) {
                alertDiv.parentNode.removeChild(alertDiv);
            }
        }, 7000);
    }, 100);
}
/* ======================================= admin header ======================================= */
/* ============ left box =========== */
// search box
function handleSearchBox() {
    const searchBox = document.querySelectorAll(".search-box");
    const searchIcon = document.querySelectorAll(".search__icon");
    const searchCloseIcon = document.querySelectorAll(
        ".search-box__close-icon"
    );

    searchIcon.forEach((element) => {
        element.onclick = () => {
            searchBox.forEach((element) => {
                if (!element.classList.contains("active")) {
                    element.classList.add("active");
                }
            });
        };
    });

    searchCloseIcon.forEach((element) => {
        element.onclick = () => {
            searchBox.forEach((element) => {
                element.classList.remove("active");
            });
        };
    });
}

// screen btn
function handleScreenButtons() {
    const screenIconExpand = document.querySelectorAll(".screen__icon--expand");
    const screenIconCompress = document.querySelectorAll(
        ".screen__icon--compress"
    );

    screenIconExpand.forEach((element) => {
        element.onclick = () => {
            screenIconCompress.forEach((element) => {
                element.classList.add("active");
            });
            screenIconExpand.forEach((element) => {
                element.classList.remove("active");
            });
            if (document.exitFullscreen) {
                document.exitFullscreen();
            }
        };
    });

    screenIconCompress.forEach((element) => {
        element.onclick = () => {
            screenIconExpand.forEach((element) => {
                element.classList.add("active");
            });
            screenIconCompress.forEach((element) => {
                element.classList.remove("active");
            });
            if (!document.fullscreenElement) {
                document.documentElement.requestFullscreen();
            }
        };
    });
}

// toggle navbar
function handleToggleNav() {
    const toggleNav = document.getElementById("toggle__nav");
    const navbar = document.getElementById("navbar");
    const main = document.getElementById("main");
    if (toggleNav) {
        toggleNav.onclick = () => {
            if (toggleNav.checked) {
                localStorage.setItem("navbarTransform", "0");
                navbar.style.transform = "translateX(0)";
                if (main.offsetWidth > 575.98) {
                    main.style.padding = "103px 20px 20px 270px";
                }
            } else {
                localStorage.setItem("navbarTransform", "-100%");
                navbar.style.transform = "translateX(-100%)";
                if (main.offsetWidth > 575.98) {
                    main.style.padding = "103px 20px 20px";
                }
            }
        };
    }
}

// right box dropdown btn
function handleRightBoxDropdown() {
    const rightBoxBtnDropdown = document.getElementById(
        "right-box__btn-dropdown"
    );
    const rightBox = document.getElementById("right-box");

    if (rightBoxBtnDropdown) {
        rightBoxBtnDropdown.onclick = () => {
            if (rightBoxBtnDropdown.classList.contains("active")) {
                rightBoxBtnDropdown.classList.remove("active");
                rightBox.style.top = "0";
            } else {
                rightBoxBtnDropdown.classList.add("active");
                rightBox.style.top = "80px";
            }
        };
    }
}

/* ======================================= nav ======================================= */
function handleNavigation() {
    const navItem = document.querySelectorAll("[data-menu]");
    if (navItem) {
        navItem.forEach((item) => {
            item.onclick = () => {
                sessionStorage.setItem(
                    "data-menu",
                    item.getAttribute("data-menu")
                );
            };
        });
    }
    // remove data-menu on logout
    const logout = document.getElementById("logout");
    if (logout) {
        logout.onclick = () => {
            sessionStorage.removeItem("data-menu");
        };
    }
}

/* ======================================= main ======================================= */
function handleFilterButton() {
    const filterBtn = document.getElementById("advanced-filter-btn");
    const handleBtn = document.getElementById("advanced-handle-btn");
    const toolBarAdvancedFilterBox = document.getElementById(
        "toolbar__advanced-filter-box"
    );
    const toolBarAdvancedHandleBox = document.getElementById(
        "toolbar__advanced-handle-box"
    );
    if (filterBtn) {
        filterBtn.onclick = () => {
            var height = sessionStorage.getItem("filterBoxHeight");
            toolBarAdvancedFilterBox.style.height =
                height === "100%" ? "0" : "100%";

            sessionStorage.setItem(
                "filterBoxHeight",
                toolBarAdvancedFilterBox.style.height
            );
            if (
                sessionStorage.getItem("handleBoxHeight") === "100%" &&
                sessionStorage.getItem("filterBoxHeight") === "100%"
            ) {
                toolBarAdvancedHandleBox.style.height = "0";
                sessionStorage.setItem("handleBoxHeight", "0");
            }
        };
    }
    if (handleBtn) {
        handleBtn.onclick = () => {
            var height = sessionStorage.getItem("handleBoxHeight");
            toolBarAdvancedHandleBox.style.height =
                height === "100%" ? "0" : "100%";

            sessionStorage.setItem(
                "handleBoxHeight",
                toolBarAdvancedHandleBox.style.height
            );
            if (
                sessionStorage.getItem("handleBoxHeight") === "100%" &&
                sessionStorage.getItem("filterBoxHeight") === "100%"
            ) {
                toolBarAdvancedFilterBox.style.height = "0";
                sessionStorage.setItem("filterBoxHeight", "0");
            }
        };
    }
}

/* ============ upload file =========== */
function handleFileUpload() {
    const imageGroup = document.getElementById("image-group");
    const imageWrapper = document.getElementById("image-wrapper");
    const uploadFile = document.querySelector("input[type='file']");
    const MAX_IMAGES = 12;
    if (uploadFile && imageGroup) {
        uploadFile.onchange = () => {
            var files = [...uploadFile.files];
            if (files.length === 0) return;
            if (files.length > MAX_IMAGES) {
                alert("error", `Chỉ upload được tối đa ${MAX_IMAGES} ảnh`);
                files = files.slice(0, MAX_IMAGES);
                return;
            }
            files.forEach((file) => {
                var thumbnailPreview = document.createElement("div");
                var imgPreview = document.createElement("img");
                var closeIcon = document.createElement("span");
                imgPreview.src = URL.createObjectURL(file);

                closeIcon.classList.add("image__close-icon");
                closeIcon.innerHTML += `<i class="fa-solid fa-circle-xmark" style="background-color: #fff; border-radius: 50%; display: block;"></i>`;

                thumbnailPreview.append(imgPreview, closeIcon);
                thumbnailPreview.classList.add("image-group__preview");

                imgPreview.onload = () => URL.revokeObjectURL(imgPreview.src);

                imageGroup.prepend(thumbnailPreview);
                closeIcon.onclick = (e) => {
                    e.target.closest(".image-group__preview").remove();
                };
            });
        };
    }
    if (uploadFile && imageWrapper) {
        console.log("helowword");
        uploadFile.onchange = () => {
            var files = [...uploadFile.files];
            if (files.length === 0) return;
            if (files.length > 1) {
                alert("error", `Chỉ upload được tối đa 1 ảnh`);
                files = files.slice(0, MAX_IMAGES);
                return;
            }
            var thumbnailPreview = document.createElement("div");
            var imgPreview = document.createElement("img");
            var closeIcon = document.createElement("span");
            imgPreview.src = URL.createObjectURL(files[0]);
            closeIcon.classList.add("image__close-icon");
            closeIcon.innerHTML += `<i class="fa-solid fa-circle-xmark" style="background-color: #fff; border-radius: 50%; display: block;"></i>`;
            thumbnailPreview.append(imgPreview, closeIcon);
            thumbnailPreview.classList.add("single-image__preview");
            imgPreview.onload = () => URL.revokeObjectURL(imgPreview.src);
            imageWrapper.firstElementChild.style.display = "none";
            imageWrapper.prepend(thumbnailPreview);
            closeIcon.onclick = (e) => {
                e.target.closest(".single-image__preview").remove();
                imageWrapper.firstElementChild.style.display = "flex";
                uploadFile.value = "";
            };
        };
    }
}

/* ============ product change =========== */
function handleProductItemChange() {
    const productItemIcon = document.querySelectorAll(".table-item__icon");
    if (productItemIcon) {
        productItemIcon.forEach((item) => {
            item.onclick = debounce(() => {
                let parent = item.parentNode;
                let ProductItemInput = parent.nextSibling;
                let currentValue = ProductItemInput.value;
                let path = item.getAttribute("path");
                parent.style.display = "none";
                ProductItemInput.style.display = "block";
                ProductItemInput.focus();
                ProductItemInput.onblur = () => {
                    if (path != "product/update-order") {
                        parent.firstChild.textContent = `${parseFloat(
                            ProductItemInput.value
                        ).toLocaleString("vi-VN")}đ`;
                    } else {
                        parent.firstChild.textContent = ProductItemInput.value;
                    }
                    parent.style.display = "block";
                    ProductItemInput.style.display = "none";
                    if (currentValue === ProductItemInput.value) {
                        return;
                    }
                    item.style.pointerEvents = "none";
                    if (path != "product/update-order") {
                        setTimeout(() => {
                            item.style.pointerEvents = "auto";
                        }, 1500);
                    }
                    fetch(`${PATH_ADMIN}/${path}?_method=PATCH`, {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            field: ProductItemInput.getAttribute("field"),
                            value: ProductItemInput.value,
                            _id: ProductItemInput.getAttribute("_id"),
                        }),
                    })
                        .then((response) => response.json())
                        .then((data) => {
                            if (data.success && !data.error) {
                                alert("success", data.message);
                            } else {
                                alert(
                                    "error",
                                    data.message || "Có lỗi xảy ra!"
                                );
                            }
                            return data;
                        })
                        .then((data) => {
                            if (data.reload) {
                                window.location.reload();
                            }
                        })
                        .catch((err) => {
                            alert("error", "Không thể kết nối đến server!");
                            console.log("Lỗi Fetch API:", err);
                        });
                };
            });
        }, 3000);
    }
}

/* ============ handle status =========== */
function handleStatusInput() {
    const btnStatusInput = document.querySelectorAll(".btn-status__input");
    if (btnStatusInput) {
        btnStatusInput.forEach((element) => {
            element.onchange = debounce(() => {
                let btnStatusDesc = element.parentNode.parentNode.nextSibling;
                let path = element.getAttribute("path");
                if (element.checked) {
                    element.value = "on";
                    btnStatusDesc.textContent = "Hoạt động";
                } else {
                    element.value = "off";
                    btnStatusDesc.textContent = "Không hoạt động";
                }
                fetch(`${PATH_ADMIN}/${path}?_method=PATCH`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        value: element.value,
                        _id: element.getAttribute("_id"),
                    }),
                })
                    .then((response) => response.json())
                    .then((data) => {
                        if (data.success && !data.error) {
                            alert("success", data.message);
                        } else {
                            alert("error", data.message || "Có lỗi xảy ra!");
                        }
                    })
                    .catch((err) => {
                        alert("error", "Không thể kết nối đến server!");
                        console.log("Lỗi Fetch API:", err);
                    });
            }, 300);
        });
    }
}

/* ============ handle featured =========== */
function handleFeaturedInput() {
    const btnFeaturedInput = document.querySelectorAll(".btn-featured__input");
    if (btnFeaturedInput) {
        btnFeaturedInput.forEach((element) => {
            element.onchange = debounce(() => {
                let btnStatusFeatured =
                    element.parentNode.parentNode.nextSibling;
                let path = element.getAttribute("path");
                if (element.checked) {
                    element.value = "on";
                    btnStatusFeatured.textContent = "Có";
                } else {
                    element.value = "off";
                    btnStatusFeatured.textContent = "Không";
                }
                fetch(`${PATH_ADMIN}/${path}?_method=PATCH`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        value: element.value,
                        _id: element.getAttribute("_id"),
                    }),
                })
                    .then((response) => response.json())
                    .then((data) => {
                        if (data.success && !data.error) {
                            alert("success", data.message);
                        } else {
                            alert("error", data.message || "Có lỗi xảy ra!");
                        }
                    })
                    .catch((err) => {
                        alert("error", "Không thể kết nối đến server!");
                        console.log("Lỗi Fetch API:", err);
                    });
            }, 300);
        });
    }
}

/* ============ handle waring form =========== */
function handleWarningFormProduct() {
    const btnDelete = document.querySelectorAll(".btn-delete");
    const btnDestroy = document.querySelectorAll(".btn-destroy");
    const formProductDelete = document.getElementById("form-product-delete");
    const warningDeleteModalBtn = document.getElementById(
        "warning-delete-modal-btn"
    );
    const warningDeleteMultipleModalBtn = document.getElementById(
        "warning-delete-multiple-modal-btn"
    );
    const warningRestoreMultipleModalBtn = document.getElementById(
        "warning-restore-multiple-modal-btn"
    );
    const warningUpdateMultipleModalBtn = document.getElementById(
        "warning-update-multiple-modal-btn"
    );
    const warningPermissionModalBtn = document.getElementById(
        "warning-permission-modal-btn"
    );
    if (formProductDelete && warningDeleteModalBtn) {
        if (btnDelete) {
            btnDelete.forEach((btn) => {
                btn.onclick = () => {
                    const idProduct = btn.getAttribute("data-bs-id");
                    console.log
                    const path = btn.getAttribute("path");
                    formProductDelete.action = `${PATH_ADMIN}/${path}/${idProduct}`;
                };
            });
        }
        if (btnDestroy) {
            btnDestroy.forEach((btn) => {
                btn.onclick = () => {
                    const idProduct = btn.getAttribute("data-bs-id");
                    const path = btn.getAttribute("path");
                    formProductDelete.action = `${PATH_ADMIN}/${path}/${idProduct}`;
                };
            });
        }
        
        warningDeleteModalBtn.onclick = () => {
            formProductDelete.submit();
        };
    }

    if (warningDeleteMultipleModalBtn) {
        warningDeleteMultipleModalBtn.onclick = () => {
            document.getElementById("handle-delete-form").submit();
        };
    }

    if (warningRestoreMultipleModalBtn) {
        warningRestoreMultipleModalBtn.onclick = () => {
            document.getElementById("handle-restore-form").submit();
        };
    }

    if (warningUpdateMultipleModalBtn) {
        warningUpdateMultipleModalBtn.onclick = () => {
            document.getElementById("handle-update-form").submit();
        };
    }

    if (warningPermissionModalBtn) {
        warningPermissionModalBtn.onclick = () => {
            document.getElementById("handle-permission-form").submit();
        };
    }
}

/* ============ handle edit form =========== */
function handleEditForm() {
    const thumbnailDeleted = document.getElementById("thumbnail-deleted");
    const imgCloseIcon = document.querySelectorAll(".image__close-icon");
    const avatarWrapper = document.getElementById("image-wrapper");
    var arr = [];
    if (imgCloseIcon && avatarWrapper) {
        imgCloseIcon.forEach((item) => {
            item.onclick = (e) => {
                e.target.closest(".single-image__preview").remove();
                avatarWrapper.firstElementChild.style.display = "flex";
                uploadFile.value = "";
            };
        });
    } else if (imgCloseIcon) {
        imgCloseIcon.forEach((item) => {
            item.onclick = (e) => {
                let imgGroupPreview = e.target.closest(".image-group__preview");
                if (!imgGroupPreview) return;
                arr = [
                    ...arr,
                    `${imgGroupPreview.firstElementChild.getAttribute("fileName")}`,
                ];
                thumbnailDeleted.value = JSON.stringify(arr);
                imgGroupPreview.remove();
            };
        });
    }
}
/* ============ handle sort product =========== */
function handleSortProduct() {
    const listProductSortIcon = document.querySelectorAll(
        ".list-product__sort-link"
    );
    listProductSortIcon.forEach((item) => {
        item.onclick = (e) => {
            e.preventDefault();
            var href = item.getAttribute("href");
            document
                .querySelectorAll("#filter-form input[type='text']")
                .forEach((node) => {
                    href += `&${encodeURIComponent(
                        node.name
                    )}=${encodeURIComponent(node.value)}`;
                });

            document.querySelectorAll("#filter-form select").forEach((node) => {
                href += `&${encodeURIComponent(node.name)}=${encodeURIComponent(
                    node.value
                )}`;
            });

            var valueRadio = "";
            document
                .querySelectorAll("#filter-form input[type='radio']")
                .forEach((node) => {
                    if (node.checked == true) {
                        valueRadio = node.value;
                    }
                });
            href += `&status=${encodeURIComponent(valueRadio)}`;

            document
                .querySelectorAll("#filter-form input[type='number']")
                .forEach((node) => {
                    href += `&${encodeURIComponent(
                        node.name
                    )}=${encodeURIComponent(node.value)}`;
                });

            window.location.href = href;
        };
    });
}

/* ============ handle select Product Item =========== */
function handleSelectProductItem() {
    const tableItem = document.querySelectorAll(".table-item__select");
    const tableItemSelectAll = document.getElementById(
        "table-item__select-all"
    );
    const listId = document.querySelectorAll(".list-id");
    if (tableItem && tableItemSelectAll) {
        var arr = [];
        tableItem.forEach((item) => {
            item.onchange = () => {
                tableItemSelectAll.checked = [...tableItem].reduce(
                    (value, item2) => item2.checked && value,
                    true
                );
                arr = JSON.parse(listId[0].value);
                var idProduct = item.getAttribute("_id");
                if (item.checked) {
                    arr = [...arr, idProduct];
                } else {
                    arr = arr.filter((item) => item != idProduct);
                }
                listId.forEach((item) => {
                    item.value = JSON.stringify(arr);
                });
            };
        });
        tableItemSelectAll.onclick = () => {
            arr = [];
            tableItem.forEach((item) => {
                item.checked = tableItemSelectAll.checked;
            });
            if (tableItemSelectAll.checked) {
                tableItem.forEach((item) => {
                    var idProduct = item.getAttribute("_id");
                    arr = [...arr, idProduct];
                });
            } else {
                arr = [];
            }
            listId.forEach((item) => {
                item.value = JSON.stringify(arr);
            });
        };
    }
}

/* ============ handle Pagination Btn =========== */
function handlePaginationBtn() {
    const btnPrev = document.getElementById("btn-prev");
    const btnNext = document.getElementById("btn-next");
    const paginationBtn = document.getElementById("pagination-btn");
    if (paginationBtn) {
        let numberOfPage = parseInt(
            paginationBtn.getAttribute("number-of-page")
        );
        let currentPage = parseInt(paginationBtn.getAttribute("current-page"));
        let currentPath = paginationBtn.getAttribute("current-path");
        if (currentPage == 1) {
            btnPrev.closest(".page-item").classList.add("disable");
        } else {
            btnPrev.closest(".page-item").classList.remove("disable");
        }
        if (currentPage == numberOfPage) {
            btnNext.closest(".page-item").classList.add("disable");
        } else {
            btnNext.closest(".page-item").classList.remove("disable");
        }
        btnNext.onclick = () => {
            if (currentPage + 1 <= numberOfPage) {
                window.location.href = `${currentPath}page=${currentPage + 1}`;
            }
        };
        btnPrev.onclick = () => {
            if (currentPage - 1 >= 1) {
                window.location.href = `${currentPath}page=${currentPage - 1}`;
            }
        };
    }
}

/* ============ handle permissions checkbox =========== */
function handlePermissionsCheckBox() {
    const tableItem = document.querySelectorAll(".table-item__label");
    const tableItemSelectAll = document.querySelectorAll(
        ".table-item__label-all"
    );
    const listRole = document.querySelectorAll(".list-role");
    if (tableItem && tableItemSelectAll) {
        tableItem.forEach((item) => {
            const input = document.getElementById(item.htmlFor);
            input.onchange = () => {
                const LabelAll = Array.from(tableItemSelectAll).find(
                    (itemAll) =>
                        itemAll.getAttribute("_id") == item.getAttribute("_id")
                );
                const inputAll = document.getElementById(LabelAll.htmlFor);
                inputAll.checked = [...tableItem].reduce((value, item2) => {
                    if (
                        item2.getAttribute("_id") !=
                        LabelAll.getAttribute("_id")
                    )
                        return value;
                    const input2 = document.getElementById(item2.htmlFor);
                    return input2.checked && value;
                }, true);
                const role = Array.from(listRole).find(
                    (input) => input.name == item.getAttribute("_id")
                );
                arr = JSON.parse(role.value);
                if (input.checked) {
                    arr = [...arr, input.value];
                } else {
                    arr = arr.filter((item) => item != input.value);
                }
                role.value = JSON.stringify(arr);
            };
        });
        tableItemSelectAll.forEach((item) => {
            const inputAll = document.getElementById(item.htmlFor);
            inputAll.onchange = () => {
                let arr = [];
                const role = Array.from(listRole).find(
                    (input) => input.name == item.getAttribute("_id")
                );
                tableItem.forEach((item2) => {
                    if (item2.getAttribute("_id") == item.getAttribute("_id")) {
                        const input = document.getElementById(item2.htmlFor);
                        input.checked = inputAll.checked;
                        if (inputAll.checked) {
                            arr = [...arr, input.value];
                        }
                    }
                });
                role.value = JSON.stringify(arr);
            };
        });
    }
}

/* ======================================= handle responsive ======================================= */
function handleResponsiveNav() {
    const toggleNav = document.getElementById("toggle__nav");
    const navbar = document.getElementById("navbar");
    const main = document.getElementById("main");

    if (toggleNav || navbar) {
        if (window.matchMedia("(max-width: 575.98px)").matches) {
            localStorage.setItem("navbarTransform", "-100%");
            toggleNav.checked = false;
            navbar.style.transform = `translateX(-100%)`;
        }
        window.addEventListener("resize", () => {
            if (window.matchMedia("(max-width: 575.98px)").matches) {
                localStorage.setItem("navbarTransform", "-100%");
                toggleNav.checked = false;
                navbar.style.transform = "translateX(-100%)";
                main.style.padding = "103px 20px 20px";
            } else {
                localStorage.setItem("navbarTransform", "0");
                toggleNav.checked = true;
                navbar.style.transform = "translateX(0)";
                main.style.padding = "103px 20px 20px 270px";
            }
        });
    }
}

// Utility function
function debounce(func, wait) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

// window load
function handleWindowOnload() {
    const toolBarAdvancedFilterBox = document.getElementById(
        "toolbar__advanced-filter-box"
    );
    const toolBarAdvancedHandleBox = document.getElementById(
        "toolbar__advanced-handle-box"
    );
    if (toolBarAdvancedHandleBox && toolBarAdvancedFilterBox) {
        var filterHeight = sessionStorage.getItem("filterBoxHeight") || 0;
        var handleHeight = sessionStorage.getItem("handleBoxHeight") || 0;
        toolBarAdvancedFilterBox.style.height = filterHeight;
        toolBarAdvancedHandleBox.style.height = handleHeight;
    }

    const toggleNav = document.getElementById("toggle__nav");
    const navbar = document.getElementById("navbar");
    const main = document.getElementById("main");
    const navbarTransform = localStorage.getItem("navbarTransform");
    if (toggleNav && navbar && main) {
        if (navbarTransform == "0") {
            toggleNav.checked = true;
            navbar.style.transform = "translateX(0)";
            if (main.offsetWidth > 575.98) {
                main.style.padding = "103px 20px 20px 270px";
            }
        } else {
            toggleNav.checked = false;
            navbar.style.transform = "translateX(-100%)";
            if (main.offsetWidth > 575.98) {
                main.style.padding = "103px 20px 20px";
            }
        }
    }

    const tableItem = document.querySelectorAll(".table-item__label");
    const tableItemSelectAll = document.querySelectorAll(
        ".table-item__label-all"
    );
    if (tableItem && tableItemSelectAll) {
        tableItemSelectAll.forEach((labelAll) => {
            const inputAll = document.getElementById(labelAll.htmlFor);
            const checkboxes = [...tableItem].filter(
                (item) =>
                    item.getAttribute("_id") === labelAll.getAttribute("_id")
            );

            inputAll.checked = checkboxes.every((item) => {
                const input = document.getElementById(item.htmlFor);
                return input.checked;
            });
        });
    }
    // navigation
    const navItem = document.querySelectorAll("[data-menu]");
    if (navItem) {
        const dataMenu = sessionStorage.getItem("data-menu") || "dashboard";
        navItem.forEach((item) => {
            if (dataMenu === item.getAttribute("data-menu")) {
                item.classList.add("active");
                const navigationItem = item.closest(".navigation-item");
                if (
                    navigationItem &&
                    navigationItem.querySelectorAll("button")
                ) {
                    const btnExpand = navigationItem.querySelectorAll("button");
                    btnExpand.forEach((btn) => {
                        btn.classList.add("active");
                        btn.classList.remove("collapsed");
                        btn.setAttribute("aria-expanded", "true");
                        const nextSibling = btn?.nextElementSibling;
                        nextSibling.classList.add("show");
                    });
                }
            } else {
                item.classList.remove("active");
            }
        });
    }

    // btn status
    const btnStatusInput = document.querySelectorAll(".btn-status__input");
    btnStatusInput.forEach((element) => {
        let btnStatusDesc = element.parentNode.parentNode.nextSibling;
        if (element.value == "on") {
            element.checked = true;
            btnStatusDesc.textContent = "Hoạt động";
        } else {
            element.checked = false;
            btnStatusDesc.textContent = "Không hoạt động";
        }
    });
    // btn featured
    const btnFeaturedInput = document.querySelectorAll(".btn-featured__input");
    btnFeaturedInput.forEach((element) => {
        let btnStatusFeatured = element.parentNode.parentNode.nextSibling;
        if (element.value == "on") {
            element.checked = true;
            btnStatusFeatured.textContent = "Có";
        } else {
            element.checked = false;
            btnStatusFeatured.textContent = "Không";
        }
    });
}

// Initialize all handlers 
function init() {
    handleWindowOnload();
    handleSearchBox();
    handleScreenButtons();
    handleToggleNav();
    handleRightBoxDropdown();
    handleNavigation();
    handleFilterButton();
    handleFileUpload();
    handleProductItemChange();
    handleStatusInput();
    handleFeaturedInput();
    handleEditForm();
    handleResponsiveNav();
    handleWarningFormProduct();
    handleSortProduct();
    handleSelectProductItem();
    handlePaginationBtn();
    handlePermissionsCheckBox();
}
document.addEventListener("DOMContentLoaded", init);
