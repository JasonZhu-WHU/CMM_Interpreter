package cmm;

public class Main {

	/** Main entry point. */
	public static void main(String args[]) {
		CMM_Interpreter interpreter;
		String file = "test/test.cmm";
		System.out.println("���ڶ�ȡ" + file + " . . .");
		try {
			interpreter = new CMM_Interpreter(new java.io.FileInputStream(file));
		} catch (java.io.FileNotFoundException e) {
			System.out.println("�ļ�����ָ��·��");
			return;
		}

		try {
			CMM_Interpreter.Start();
			((SimpleNode) interpreter.rootNode()).dump("\t");
		} catch (ParseException e) {
			System.out.println("�ʷ����﷨�������ִ���");
			e.printStackTrace();
		} catch (Exception e1) {
			System.out.println("���ֲ���ԭ�����");
			e1.printStackTrace();
		}
	}
}
