/// <reference path="json2.min.js"/>

var JSMController = (function (window, document) {
	'use strict';
	var controller = {};
	// private properties
	var VARCONTAINERID = "JSMVariables";
	var FUNCCONTAINERID = "JSMFunctions";
	var PageLoadEvent = { Init: 0, Ready: 1 };
	var queue = [];

	// private functions
	function GetEmbeddedJson(id) {
		var json = document.getElementById(id).innerHTML;
		var isComment = (json.indexOf("<") === 0) || (json.indexOf("&lt;") === 0); //check if we are in another browser than IE
		if (!isComment) // IE detected, return Json
			return json.replace(/\&amp;/g, "&"); //ie is messing with some templates
		var hasLtEntity = (json.indexOf("&lt;") === 0); // Fix for Firefox 3.6
		return json.substring(hasLtEntity ? 20 : 14, (json.length - (hasLtEntity ? 18 : 12))); //return substring without html comment tags
	};
	function ParseCodeBehindJson(json) {
		return json ? JSON.parse(json) : null;
	};
	function FillCodeBehindJsVariables(variables) {
		if (!variables)
			return;
		for (var variableName in variables) {
			var value = variables[variableName];
			var variableParts = variableName.split(".");
			var current = window;
			// If the variable contains '.' we have to get or create the literals iteratively
			for (var i = 0; i < variableParts.length - 1; ++i) {
				//literal not found --> create new one
				if (!current[variableParts[i]]) {
					current[variableParts[i]] = {};
				}
				current = current[variableParts[i]];
			}
			// finally set the value
			current[variableParts[variableParts.length - 1]] = value;
		}
	};
	function FillCodeBehindJavaScriptFunctions(functions, pageLoadEvent) {
		if (!functions)
			return;
		for (var i = 0; i < functions.length; ++i) {
			var functionContainer = functions[i];
			var functionNameParts = functionContainer.Function.split(".");
			var currentFunction = window;
			// If the function name contains '.' we have to get the function iteratively
			for (var j = 0; j < functionNameParts.length; ++j) {
				currentFunction = currentFunction[functionNameParts[j]];
			}
			if (typeof currentFunction !== "function")
				throw new Error(functionContainer.Function + " is not a valid function.");
			// store the function reference in the queue
			queue.push({ Type: pageLoadEvent, Function: currentFunction, Parameters: functionContainer.Parameters });
		}
	};
	function Initialize(javaScriptVariables, javaScriptFunctions, pageLoadEvent) {
		FillCodeBehindJsVariables(javaScriptVariables);
		FillCodeBehindJavaScriptFunctions(javaScriptFunctions, pageLoadEvent);
		RunQueue(pageLoadEvent);
	};
	function RunQueue(pageLoadEvent) {
		// run all functions in the queue for the given page load event
		for (var i = 0; i < queue.length; ++i) {
			var currentFunction = queue[i];
			if (currentFunction.Type === pageLoadEvent) {
				currentFunction.Function.apply(window, currentFunction.Parameters);
			}
		}
	};

	// public functions
	controller.Init = function (javaScriptVariables, javaScriptFunctions) {
		Initialize(javaScriptVariables, javaScriptFunctions, PageLoadEvent.Init);
	};
	controller.InitReady = function () {
		Initialize(ParseCodeBehindJson(GetEmbeddedJson(VARCONTAINERID)), ParseCodeBehindJson(GetEmbeddedJson(FUNCCONTAINERID)), PageLoadEvent.Ready);
	};

	return controller;
}(window, document));