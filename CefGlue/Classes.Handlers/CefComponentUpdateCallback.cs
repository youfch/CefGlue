namespace Xilium.CefGlue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Xilium.CefGlue.Interop;

    /// <summary>
    /// Called when the component update operation completes.
    /// </summary>
    public abstract unsafe partial class CefComponentUpdateCallback
    {
        private void on_complete(cef_component_update_callback_t* self, cef_string_t* component_id, CefComponentUpdateError error)
        {
            CheckSelf(self);

            var mComponentId = cef_string_t.ToString(component_id);

            OnComplete(mComponentId, error);
        }

        /// <summary>
        /// Called when the component update operation completes.
        /// |component_id| is the ID of the component that was updated.
        /// |error| is CEF_COMPONENT_UPDATE_ERROR_NONE on success, or an error code.
        /// </summary>
        protected virtual void OnComplete(string componentId, CefComponentUpdateError error)
        {
        }
    }
}
