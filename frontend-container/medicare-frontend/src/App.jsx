import { useState } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import './styles/App.css'
import './styles/styles.css'
import Login from './pages/Login'
import PatientDashboard from './pages/Dashboard/PatientDashboard'
import Scheduler from './pages/Scheduler/Scheduler'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="min-h-screen bg-gray-100">
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<PatientDashboard />} />
          <Route path="/schedule" element={<Scheduler />} />
          <Route path="/login" element={<Login />} />
        </Routes>
      </BrowserRouter>
    </div>
  )
}

export default App
