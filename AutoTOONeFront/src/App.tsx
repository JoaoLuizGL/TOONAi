import React, { useState, useRef } from 'react';
import { 
  FileText, 
  Upload, 
  RefreshCw, 
  Check, 
  Copy, 
  Download, 
  AlertCircle,
  ArrowRight,
  Moon,
  Sun,
  FileJson // Added fallback icon
} from 'lucide-react';

// --- INSTRUÇÕES PARA LOGO ---
// 1. Crie uma pasta 'assets' dentro de 'src'
// 2. Coloque sua imagem lá com o nome 'logo.png'
// 3. Descomente a linha abaixo:
import logoSrc from './assets/logo.png'; 

// Tipagem para as abas
type TabType = 'text' | 'file';

export default function App() {
  // --- State ---
  // Dark mode is default (true)
  const [isDarkMode, setIsDarkMode] = useState<boolean>(true);
  const [activeTab, setActiveTab] = useState<TabType>('text');
  const [jsonInput, setJsonInput] = useState<string>('');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [toonOutput, setToonOutput] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [copySuccess, setCopySuccess] = useState<boolean>(false);

  // Variable to simulate the imported image. 
  // Once you uncomment the import above, assign logoSrc to this variable if needed, 
  // or just use logoSrc directly in the JSX.
  const logoImage = logoSrc; // Change this to 'logoSrc' after uncommenting the import

  // --- Refs ---
  const fileInputRef = useRef<HTMLInputElement>(null);

  // --- Input Handlers ---
  const handleTextChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setJsonInput(e.target.value);
    setError(null);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setSelectedFile(e.target.files[0]);
      setError(null);
    }
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      setSelectedFile(e.dataTransfer.files[0]);
      setError(null);
    }
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
  };

  // --- Conversion Logic (API) ---
  const handleConvert = async () => {
    setIsLoading(true);
    setError(null);
    setToonOutput('');

    try {
      let response;
      const baseUrl = 'http://localhost:5108/Toon/convert';

      if (activeTab === 'text') {
        // Basic Validation
        if (!jsonInput.trim()) throw new Error("Please enter a JSON to convert.");
        try {
            JSON.parse(jsonInput);
        } catch (e) {
            throw new Error("The entered text is not a valid JSON.");
        }

        // Text Route
        response = await fetch(baseUrl, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(jsonInput),
        });

      } else {
        // File Route
        if (!selectedFile) throw new Error("Please select a JSON file.");

        const formData = new FormData();
        formData.append('file', selectedFile);

        response = await fetch(`${baseUrl}/file`, {
          method: 'POST',
          body: formData,
        });
      }

      if (!response.ok) {
        throw new Error(`API Error: ${response.statusText}`);
      }

      // Backend returns Plain Text
      const textResult = await response.text();
      setToonOutput(textResult);

    } catch (err: any) {
      setError(err.message || "An unknown error occurred");
    } finally {
      setIsLoading(false);
    }
  };

  // --- Extra Features ---
  const handleCopy = () => {
    navigator.clipboard.writeText(toonOutput);
    setCopySuccess(true);
    setTimeout(() => setCopySuccess(false), 2000);
  };

  const handleDownload = () => {
    const element = document.createElement("a");
    const file = new Blob([toonOutput], { type: 'text/plain' });
    element.href = URL.createObjectURL(file);
    element.download = "converted.toon";
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
  };

  const toggleTheme = () => setIsDarkMode(!isDarkMode);

  // --- Render ---
  // The outer div controls the 'dark' class scope
  return (
    <div className={isDarkMode ? "dark" : ""}>
      <div className="min-h-screen bg-slate-50 dark:bg-slate-900 text-slate-800 dark:text-slate-100 font-sans p-4 md:p-8 transition-colors duration-300">
        
        {/* Theme Toggle (Top Right) */}
        <div className="absolute top-4 right-4">
          <button 
            onClick={toggleTheme}
            className="p-2 rounded-full bg-slate-200 dark:bg-slate-800 text-slate-700 dark:text-slate-300 hover:bg-slate-300 dark:hover:bg-slate-700 transition-colors"
          >
            {isDarkMode ? <Sun className="w-5 h-5" /> : <Moon className="w-5 h-5" />}
          </button>
        </div>

        <div className="max-w-4xl mx-auto space-y-6">
          
          {/* Header */}
          <header className="text-center mb-8">
            <h1 className="text-3xl md:text-4xl font-bold text-slate-900 dark:text-white flex items-center justify-center gap-4">
              {/* Logo Container */}
              <div className="w-12 h-12 bg-indigo-100 dark:bg-slate-800 rounded-lg flex items-center justify-center overflow-hidden shadow-sm">
                 {logoImage ? (
                   <img 
                     src={logoImage} 
                     alt="TOONAi Logo" 
                     className="w-full h-full object-cover"
                   />
                 ) : (
                   /* Fallback Icon when no image is available */
                   <div className="w-12 h-12 flex items-center justify-center bg-indigo-600 text-white font-bold">
                      <FileJson className="w-7 h-7" />
                   </div>
                 )}
              </div>
              TOONAi
            </h1>
            <p className="text-slate-500 dark:text-slate-400 mt-2">Convert JSON data structures to TOON format effortlessly.</p>
          </header>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">
            
            {/* Left Column: INPUT */}
            <div className="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-slate-200 dark:border-slate-700 overflow-hidden flex flex-col h-[500px] transition-colors">
              {/* Tabs */}
              <div className="flex border-b border-slate-200 dark:border-slate-700">
                <button
                  onClick={() => setActiveTab('text')}
                  className={`flex-1 py-3 px-4 text-sm font-medium flex items-center justify-center gap-2 transition-colors ${
                    activeTab === 'text' 
                      ? 'bg-white dark:bg-slate-800 text-indigo-600 dark:text-indigo-400 border-b-2 border-indigo-600 dark:border-indigo-400' 
                      : 'bg-slate-50 dark:bg-slate-900 text-slate-500 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700'
                  }`}
                >
                  <FileText className="w-4 h-4" /> Text Input
                </button>
                <button
                  onClick={() => setActiveTab('file')}
                  className={`flex-1 py-3 px-4 text-sm font-medium flex items-center justify-center gap-2 transition-colors ${
                    activeTab === 'file' 
                      ? 'bg-white dark:bg-slate-800 text-indigo-600 dark:text-indigo-400 border-b-2 border-indigo-600 dark:border-indigo-400' 
                      : 'bg-slate-50 dark:bg-slate-900 text-slate-500 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700'
                  }`}
                >
                  <Upload className="w-4 h-4" /> File Upload
                </button>
              </div>

              {/* Tab Content */}
              <div className="p-4 flex-1 flex flex-col">
                {activeTab === 'text' ? (
                  <textarea
                    className="w-full h-full p-4 bg-slate-50 dark:bg-slate-900 text-slate-800 dark:text-slate-200 border border-slate-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none resize-none font-mono text-sm placeholder:text-slate-400 dark:placeholder:text-slate-600"
                    placeholder='Paste your JSON here... Ex: { "name": "example" }'
                    value={jsonInput}
                    onChange={handleTextChange}
                  />
                ) : (
                  <div 
                    className="flex-1 border-2 border-dashed border-slate-300 dark:border-slate-600 rounded-lg flex flex-col items-center justify-center bg-slate-50 dark:bg-slate-900/50 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors cursor-pointer relative"
                    onDrop={handleDrop}
                    onDragOver={handleDragOver}
                    onClick={() => fileInputRef.current?.click()}
                  >
                    <input 
                      type="file" 
                      className="hidden" 
                      ref={fileInputRef} 
                      onChange={handleFileChange}
                      accept=".json,.txt"
                    />
                    
                    {selectedFile ? (
                      <div className="text-center p-4">
                        <div className="w-12 h-12 bg-indigo-100 dark:bg-indigo-900/30 rounded-full flex items-center justify-center mx-auto mb-2">
                          <Check className="w-6 h-6 text-indigo-600 dark:text-indigo-400" />
                        </div>
                        <p className="font-medium text-slate-700 dark:text-slate-300">{selectedFile.name}</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">{(selectedFile.size / 1024).toFixed(2)} KB</p>
                        <button 
                          onClick={(e) => { e.stopPropagation(); setSelectedFile(null); }}
                          className="mt-4 text-xs text-red-500 hover:text-red-400 hover:underline"
                        >
                          Remove file
                        </button>
                      </div>
                    ) : (
                      <div className="text-center p-4 pointer-events-none">
                        <Upload className="w-10 h-10 text-slate-400 dark:text-slate-500 mx-auto mb-3" />
                        <p className="text-sm text-slate-600 dark:text-slate-400 font-medium">Click or Drag & Drop a file</p>
                        <p className="text-xs text-slate-400 dark:text-slate-500 mt-1">Supports .json files</p>
                      </div>
                    )}
                  </div>
                )}
                
                {/* Error Message */}
                {error && (
                  <div className="mt-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 rounded-lg text-sm flex items-start gap-2">
                    <AlertCircle className="w-4 h-4 mt-0.5 flex-shrink-0" />
                    <span>{error}</span>
                  </div>
                )}

                {/* Convert Button */}
                <button
                  onClick={handleConvert}
                  disabled={isLoading}
                  className="mt-4 w-full bg-indigo-600 hover:bg-indigo-700 disabled:bg-indigo-400 disabled:cursor-not-allowed text-white font-semibold py-3 px-4 rounded-lg transition-colors flex items-center justify-center gap-2 shadow-sm"
                >
                  {isLoading ? (
                    <>
                      <RefreshCw className="w-5 h-5 animate-spin" /> Processing...
                    </>
                  ) : (
                    <>
                      Convert to TOON <ArrowRight className="w-5 h-5" />
                    </>
                  )}
                </button>
              </div>
            </div>

            {/* Right Column: OUTPUT */}
            <div className="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-slate-200 dark:border-slate-700 overflow-hidden flex flex-col h-[500px] transition-colors">
              <div className="bg-slate-100 dark:bg-slate-900/50 px-4 py-3 border-b border-slate-200 dark:border-slate-700 flex justify-between items-center">
                <span className="text-sm font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wide">
                  TOON Result
                </span>
                <div className="flex gap-2">
                  <button
                    onClick={handleCopy}
                    disabled={!toonOutput}
                    title="Copy text"
                    className="p-2 text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-white dark:hover:bg-slate-800 rounded-md transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {copySuccess ? <Check className="w-4 h-4" /> : <Copy className="w-4 h-4" />}
                  </button>
                  <button
                    onClick={handleDownload}
                    disabled={!toonOutput}
                    title="Download .toon file"
                    className="p-2 text-slate-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-white dark:hover:bg-slate-800 rounded-md transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <Download className="w-4 h-4" />
                  </button>
                </div>
              </div>
              
              <div className="flex-1 relative">
                <textarea
                  readOnly
                  className="w-full h-full p-4 bg-slate-50 dark:bg-slate-900 text-slate-800 dark:text-slate-200 font-mono text-sm resize-none outline-none placeholder:text-slate-400 dark:placeholder:text-slate-600"
                  value={toonOutput}
                  placeholder="The conversion result will appear here..."
                />
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  );
}