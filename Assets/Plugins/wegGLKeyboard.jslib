mergeInto(LibraryManager.library, {
    focusHandleAction: function(_name, _str, _promptHeader){
        if(UnityLoader.SystemInfo.mobile == true){
            var _inputTextData = prompt(_promptHeader, Pointer_stringify(_str));
            if (_inputTextData == null || _inputTextData == "") {
                //canceled text
            } else {
                //send data to unity
                SendMessage(Pointer_stringify(_name), 'ReceiveInputData', _inputTextData);
            }  
        }
    },
});