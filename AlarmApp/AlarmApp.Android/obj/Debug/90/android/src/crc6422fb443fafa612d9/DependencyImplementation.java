package crc6422fb443fafa612d9;


public class DependencyImplementation
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("AlarmApp.Droid.DependencyImplementation, AlarmApp.Android", DependencyImplementation.class, __md_methods);
	}


	public DependencyImplementation ()
	{
		super ();
		if (getClass () == DependencyImplementation.class) {
			mono.android.TypeManager.Activate ("AlarmApp.Droid.DependencyImplementation, AlarmApp.Android", "", this, new java.lang.Object[] {  });
		}
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
