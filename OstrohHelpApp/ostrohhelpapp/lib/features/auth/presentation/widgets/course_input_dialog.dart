import 'package:flutter/material.dart';
import 'package:easy_localization/easy_localization.dart';

class CourseInputDialog extends StatefulWidget {
  final String userId;
  final Function(String userId, String course) onSubmit;

  const CourseInputDialog({
    Key? key,
    required this.userId,
    required this.onSubmit,
  }) : super(key: key);

  @override
  State<CourseInputDialog> createState() => _CourseInputDialogState();
}

class _CourseInputDialogState extends State<CourseInputDialog> {
  final _courseController = TextEditingController();
  bool _isValid = false;

  final _courseRegExp = RegExp(r'^[А-ЯA-Zа-яa-zІіЇїЄєҐґ-]+-\d+$'); // КН-33, АБС-3, etc.

  @override
  void initState() {
    super.initState();
    _courseController.addListener(_validateInput);
  }

  @override
  void dispose() {
    _courseController.dispose();
    super.dispose();
  }

  void _validateInput() {
    setState(() {
      _isValid = _courseRegExp.hasMatch(_courseController.text.trim());
    });
  }

  @override
  Widget build(BuildContext context) {
    return WillPopScope(
      onWillPop: () async => false,
      child: AlertDialog(
        title: Text('auth.courseDialog.title'.tr()),
        content: TextField(
          controller: _courseController,
          decoration: InputDecoration(
            hintText: 'auth.courseDialog.hint'.tr(),
            border: OutlineInputBorder(),
            helperText: 'auth.courseDialog.helper'.tr(),
          ),
          autofocus: true,
        ),
        actions: [
          TextButton(
            onPressed: _isValid
                ? () {
                    widget.onSubmit(widget.userId, _courseController.text.trim());
                    Navigator.of(context).pop();
                  }
                : null,
            child: Text('common.submit'.tr()),
          ),
        ],
      ),
    );
  }
} 
