import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './styles/App.css'
import Login from './pages/Login/Login'
import ForgotPassword from './pages/Login/ForgotPassword'
import ForgotCardNumber from './pages/Login/ForgotCardNumber'
import ChoosePlan from './pages/ChoosePlan'
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom'

function App() {
  const [count, setCount] = useState(0)

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/forgot-password" element={<ForgotPassword/>} />
        <Route path="/home" element={<h1>Home</h1>} />
        <Route path="forgot-card" element={<ForgotCardNumber />} />
        <Route path="/choose-plan" element={<ChoosePlan />} />
      </Routes>
    </Router>
  )
}

export default App
