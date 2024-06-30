import 'package:editor/game_project/new_project.dart';
import 'package:flutter/material.dart';

class OpenProject extends StatefulWidget {
  const OpenProject({super.key});

  @override
  State<OpenProject> createState() => _OpenProjectState();
}

class _OpenProjectState extends State<OpenProject> {
  Route _newProject(Widget nextPage) {
    return PageRouteBuilder(
      pageBuilder: (context, animation, secondaryAnimation) => nextPage,
      transitionsBuilder: (context, animation, secondaryAnimation, child) {
        const begin = Offset(1, 0);
        const end = Offset.zero;
        const curve = Curves.easeInOut;

        var tween = Tween(begin: begin, end: end).chain(
          CurveTween(curve: curve),
        );
        var offsetAnimation = animation.drive(tween);

        return SlideTransition(position: offsetAnimation, child: child);
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        children: [
          Row(
            children: [
              Padding(
                padding: const EdgeInsets.all(16),
                child: ElevatedButton(
                  onPressed: () {
                    Navigator.push(context, _newProject(const NewProject()));
                  },
                  child: const Text(
                    "New Project",
                    style: TextStyle(fontSize: 18),
                  ),
                ),
              )
            ],
          ),
        ],
      ),
    );
  }
}
