from GameProject.ProjectBrowserDialog import ProjectBrowserDialog

pb = ProjectBrowserDialog()
pb.setup()
pb.draw()
result = pb.get_result()

if result is not None:
    print(result)