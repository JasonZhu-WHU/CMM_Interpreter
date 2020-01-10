package cmm;

public class Main {

	/** Main entry point. */
	public static void main(String args[]) {
		CMM_Interpreter interpreter;
		String file = "test/test.cmm";
		System.out.println("正在读取" + file + " . . .");
		try {
			interpreter = new CMM_Interpreter(new java.io.FileInputStream(file));
		} catch (java.io.FileNotFoundException e) {
			System.out.println("文件不在指定路径");
			return;
		}

		try {
			CMM_Interpreter.Start();
			((SimpleNode) interpreter.rootNode()).dump("\t");
		} catch (ParseException e) {
			System.out.println("词法或语法分析出现错误");
			e.printStackTrace();
		} catch (Exception e1) {
			System.out.println("出现不明原因错误");
			e1.printStackTrace();
		}
	}
}
