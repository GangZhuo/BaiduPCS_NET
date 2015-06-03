using System;

namespace BaiduPCS_NET
{
    public enum PcsRes
    {
        PCS_NONE = -1,
        PCS_OK = 0,
        PCS_FAIL,
        PCS_LOGIN,
        PCS_NOT_LOGIN,
        PCS_UNKNOWN_OPT,
        PCS_NO_BDSTOKEN,
        PCS_NETWORK_ERROR,
        PCS_WRONG_RESPONSE,
        PCS_NO_CAPTCHA_FUNC,
        PCS_GET_CAPTCHA_FAIL,
        PCS_ALLOC_MEMORY,
        PCS_BUILD_POST_DATA,
        PCS_BUILD_URL,
        PCS_NO_LIST,
        PCS_CREATE_OBJ,
        PCS_NOT_EXIST,
        PCS_CLONE_OBJ,
        PCS_WRONG_ARGS,



    }
}
