function handleProduct() {
    const countdown = document.getElementById("countdown");
    if (countdown) {
        var minute = 59;
        var second = 60;
        var hour = 11;
        const box = countdown.querySelectorAll("strong");
        setInterval(() => {
            box[2].innerText = String(second--).padStart(2, "0");
            box[1].innerText = String(minute).padStart(2, "0");
            box[0].innerText = String(hour).padStart(2, "0");
            if (hour == 0 && minute == 0 && second == 0) {
                hour = 12;
            }
            if (minute == 0 && second == 0) {
                minute = 60;
                --hour;
            }
            if (second == 0) {
                --minute;
                second = 60;
            }
        }, 1000);
    }
}

function handleCollection() {
    const checkbox = document.querySelectorAll(
        ".collection-item__thumbnail input[type='checkbox']"
    );
    if (checkbox) {
        checkbox.forEach((item) => {
            item.addEventListener("change", () => {
                if (item.checked == true) {
                    checkbox.forEach((item2) => {
                        if (item != item2) {
                            item2.checked = false;
                        }
                    });
                }
            });
        });
    }
}

async function handleAuth() {
    const passwordIcon = document.querySelectorAll(".password__icon");
    if (passwordIcon) {
        passwordIcon.forEach((element) => {
            element.onclick = () => {
                const passwordInput = element
                    .closest(".password__group")
                    .querySelector("input");
                if (passwordInput.type === "password") {
                    passwordInput.type = "text";
                    element.firstChild.classList.remove("fa-eye-slash");
                    element.firstChild.classList.add("fa-eye");
                } else {
                    passwordInput.type = "password";
                    element.firstChild.classList.remove("fa-eye");
                    element.firstChild.classList.add("fa-eye-slash");
                }
            };
        });
    }

    const registerLinkBtn = document.getElementById("register-link__btn");
    const loginLinkBtnRegister = document.getElementById(
        "login-link__btn--register"
    );
    const loginLinkBtnForgot = document.getElementById(
        "login-link__btn--forgot"
    );
    const forgotLinkBtn = document.getElementById("forgot-link__btn");
    if (
        registerLinkBtn &&
        loginLinkBtnRegister &&
        forgotLinkBtn &&
        loginLinkBtnForgot
    ) {
        const auth = document.getElementById("auth");
        loginLinkBtnRegister.onclick = () => {
            auth.style.transform = "translateX(calc(-200% / 3))";
        };
        registerLinkBtn.onclick = () => {
            auth.style.transform = "translateX(calc(-100% / 3))";
        };
        forgotLinkBtn.onclick = () => {
            auth.style.transform = "translateX(calc(-100% / 3))";
        };
        loginLinkBtnForgot.onclick = () => {
            auth.style.transform = "translateX(0)";
        };
    }

    const authFrom = document.querySelectorAll(".authForm");
    if (authFrom) {
        authFrom.forEach((item) => {
            const inputData = item.querySelectorAll("input");
            inputData.forEach((input) => {
                input.onkeyup = () => {
                    if (input.value.trim() != "") {
                        input.classList.remove("error");
                        if (
                            input.name !== "password" &&
                            input.name !== "passwordConfirm"
                        ) {
                            input.nextElementSibling.style.visibility =
                                "hidden";
                        } else {
                            input.parentNode.nextElementSibling.style.visibility =
                                "hidden";
                        }
                    }
                };
            });
        });
    }

    const btnSubmit = document.querySelectorAll(".btn-submit-auth");
    if (btnSubmit) {
        btnSubmit.forEach((item) => {
            item.onclick = async (e) => {
                const form = item.closest("form");
                const inputData = form.querySelectorAll("input");
                inputData.forEach((input) => {
                    if (input.value.trim() === "") {
                        e.preventDefault();
                        input.classList.add("error");
                        if (
                            input.name !== "password" &&
                            input.name !== "passwordConfirm"
                        ) {
                            input.nextElementSibling.style.visibility =
                                "visible";
                        } else {
                            input.parentNode.nextElementSibling.style.visibility =
                                "visible";
                        }
                    }
                });
            };
        });
    }

    const btnConfirm = document.querySelectorAll(".btn-confirm");
    if (btnConfirm) {
        btnConfirm.forEach((btn) => {
            btn.onclick = () => {
                const form = btn.closest("form");
                const inputEmail = form.querySelector("input[type=email]");
                if (inputEmail && inputEmail.value.trim() === "") {
                    inputEmail.classList.add("error");
                    inputEmail.nextElementSibling.style.visibility = "visible";
                }
            };
        });
    }
}

function handleQuantityControl() {
    const quantityControls = document.querySelectorAll(".quantity-control");
    if (quantityControls) {
        quantityControls.forEach((item) => {
            const quantityControlBtnDecrease = item.querySelector(
                ".quantity-control__btn--decrease"
            );
            const quantityControlBtnIncrease = item.querySelector(
                ".quantity-control__btn--increase"
            );
            const quantityControlInput = item.querySelector(
                ".quantity-control__input"
            );
            quantityControlBtnDecrease.onclick = () => {
                if (quantityControlInput.value > 1) {
                    quantityControlInput.value--;
                }
            };
            quantityControlBtnIncrease.onclick = () => {
                quantityControlInput.value++;
            };
        });
    }
}

function init() {
    handleProduct();
    handleCollection();
    handleAuth();
    handleQuantityControl();
}
init();
